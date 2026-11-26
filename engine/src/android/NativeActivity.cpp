#include <android/log.h>
#include <android_native_app_glue.h>
#include <EGL/egl.h>
#include <GLES3/gl3.h>

namespace {

constexpr const char* kLogTag = "JASSIM.Engine";

struct AndroidRuntime {
    EGLDisplay display{EGL_NO_DISPLAY};
    EGLSurface surface{EGL_NO_SURFACE};
    EGLContext context{EGL_NO_CONTEXT};
    int width{0};
    int height{0};

    bool initialize(ANativeWindow* window) {
        display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
        if (display == EGL_NO_DISPLAY || !eglInitialize(display, nullptr, nullptr)) {
            __android_log_print(ANDROID_LOG_ERROR, kLogTag, "EGL display initialization failed");
            return false;
        }

        const EGLint configAttributes[] = {
            EGL_RENDERABLE_TYPE, EGL_OPENGL_ES3_BIT,
            EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
            EGL_RED_SIZE, 8, EGL_GREEN_SIZE, 8,
            EGL_BLUE_SIZE, 8, EGL_ALPHA_SIZE, 8,
            EGL_DEPTH_SIZE, 16,
            EGL_NONE
        };
        EGLConfig config{};
        EGLint count = 0;
        if (!eglChooseConfig(display, configAttributes, &config, 1, &count) || count < 1) {
            shutdown();
            __android_log_print(ANDROID_LOG_ERROR, kLogTag, "No compatible GLES3 EGL config");
            return false;
        }

        const EGLint contextAttributes[] = {EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE};
        context = eglCreateContext(display, config, EGL_NO_CONTEXT, contextAttributes);
        surface = eglCreateWindowSurface(display, config, window, nullptr);
        if (context == EGL_NO_CONTEXT || surface == EGL_NO_SURFACE ||
            !eglMakeCurrent(display, surface, surface, context)) {
            shutdown();
            __android_log_print(ANDROID_LOG_ERROR, kLogTag, "GLES3 context/surface creation failed");
            return false;
        }

        eglQuerySurface(display, surface, EGL_WIDTH, &width);
        eglQuerySurface(display, surface, EGL_HEIGHT, &height);
        glViewport(0, 0, width, height);
        __android_log_print(ANDROID_LOG_INFO, kLogTag, "GLES3 context ready: %dx%d", width, height);
        return true;
    }

    void draw() const {
        if (display == EGL_NO_DISPLAY || surface == EGL_NO_SURFACE) return;
        glViewport(0, 0, width, height);
        glClearColor(0.012F, 0.025F, 0.075F, 1.0F);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        eglSwapBuffers(display, surface);
    }

    void shutdown() {
        if (display != EGL_NO_DISPLAY) {
            eglMakeCurrent(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
            if (context != EGL_NO_CONTEXT) eglDestroyContext(display, context);
            if (surface != EGL_NO_SURFACE) eglDestroySurface(display, surface);
            eglTerminate(display);
        }
        display = EGL_NO_DISPLAY;
        surface = EGL_NO_SURFACE;
        context = EGL_NO_CONTEXT;
        width = height = 0;
    }

    ~AndroidRuntime() { shutdown(); }
};

void handleCommand(android_app* app, int32_t command) {
    auto* runtime = static_cast<AndroidRuntime*>(app->userData);
    if (runtime == nullptr) return;

    if (command == APP_CMD_INIT_WINDOW && app->window != nullptr) {
        runtime->shutdown();
        runtime->initialize(app->window);
    } else if (command == APP_CMD_TERM_WINDOW) {
        runtime->shutdown();
    }
}

} // namespace

void android_main(android_app* app) {
    app_dummy();
    AndroidRuntime runtime;
    app->userData = &runtime;
    app->onAppCmd = handleCommand;

    while (!app->destroyRequested) {
        android_poll_source* source = nullptr;
        const int events = ALooper_pollOnce(16, nullptr, nullptr,
                                            reinterpret_cast<void**>(&source));
        if (events >= 0 && source != nullptr) source->process(app, source);
        runtime.draw();
    }

    app->userData = nullptr;
}
