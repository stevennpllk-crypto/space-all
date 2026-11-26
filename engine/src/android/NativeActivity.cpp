#include <android/log.h>
#include <android_native_app_glue.h>
#include <android/input.h>
#include <EGL/egl.h>
#include <GLES3/gl3.h>

#include "jassim/platform/InputState.hpp"
#include "jassim/render/Camera.hpp"
#include "jassim/render/Mesh.hpp"
#include "jassim/render/Material.hpp"
#include "jassim/render/TextureData.hpp"

#include <cmath>
#include <cstddef>
#include <cstdint>
#include <limits>

namespace {

constexpr const char* kLogTag = "JASSIM.Engine";

constexpr const char* kVertexShader = R"(#version 300 es
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUv;
uniform mat4 uViewProjection;
out vec3 vNormal;
out vec2 vUv;
void main() {
    vUv = aUv;
    gl_Position = uViewProjection * vec4(aPosition, 1.0);
    vNormal = aNormal;
})";

constexpr const char* kFragmentShader = R"(#version 300 es
precision mediump float;
in vec3 vNormal;
in vec2 vUv;
uniform sampler2D uAlbedo;
uniform vec3 uAlbedoTint;
uniform float uAmbientStrength;
uniform float uDiffuseStrength;
uniform float uUvScale;
out vec4 outColor;
void main() {
    vec3 normal = normalize(vNormal);
    vec3 lightDirection = normalize(vec3(-0.45, 0.65, 0.60));
    float diffuse = max(dot(normal, lightDirection), 0.0);
    float illumination = uAmbientStrength + diffuse * uDiffuseStrength;
    vec3 albedo = texture(uAlbedo, vUv * uUvScale).rgb * uAlbedoTint;
    outColor = vec4(albedo * illumination, 1.0);
})";

GLuint compileShader(GLenum type, const char* source) {
    const GLuint shader = glCreateShader(type);
    if (shader == 0) return 0;
    glShaderSource(shader, 1, &source, nullptr);
    glCompileShader(shader);
    GLint compiled = GL_FALSE;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
    if (compiled == GL_TRUE) return shader;

    GLint logLength = 0;
    glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &logLength);
    if (logLength > 1) {
        char log[1024]{};
        glGetShaderInfoLog(shader, sizeof(log), nullptr, log);
        __android_log_print(ANDROID_LOG_ERROR, kLogTag, "Shader compile failed: %s", log);
    }
    glDeleteShader(shader);
    return 0;
}

GLuint createProgram() {
    const GLuint vertex = compileShader(GL_VERTEX_SHADER, kVertexShader);
    const GLuint fragment = compileShader(GL_FRAGMENT_SHADER, kFragmentShader);
    if (vertex == 0 || fragment == 0) {
        if (vertex) glDeleteShader(vertex);
        if (fragment) glDeleteShader(fragment);
        return 0;
    }

    const GLuint program = glCreateProgram();
    glAttachShader(program, vertex);
    glAttachShader(program, fragment);
    glLinkProgram(program);
    glDeleteShader(vertex);
    glDeleteShader(fragment);

    GLint linked = GL_FALSE;
    glGetProgramiv(program, GL_LINK_STATUS, &linked);
    if (linked == GL_TRUE) return program;

    char log[1024]{};
    glGetProgramInfoLog(program, sizeof(log), nullptr, log);
    __android_log_print(ANDROID_LOG_ERROR, kLogTag, "Program link failed: %s", log);
    glDeleteProgram(program);
    return 0;
}

struct AndroidRuntime {
    EGLDisplay display{EGL_NO_DISPLAY};
    EGLSurface surface{EGL_NO_SURFACE};
    EGLContext context{EGL_NO_CONTEXT};
    int width{0};
    int height{0};
    GLuint program{0};
    GLuint vertexArray{0};
    GLuint vertexBuffer{0};
    GLuint indexBuffer{0};
    GLuint albedoTexture{0};
    GLsizei indexCount{0};
    GLint viewProjectionLocation{-1};
    GLint albedoLocation{-1};
    GLint albedoTintLocation{-1};
    GLint ambientStrengthLocation{-1};
    GLint diffuseStrengthLocation{-1};
    GLint uvScaleLocation{-1};
    jassim::platform::InputState input{};
    jassim::render::Camera camera{};
    jassim::render::Material planetMaterial{};

    bool createSceneResources() {
        program = createProgram();
        if (program == 0) return false;
        viewProjectionLocation = glGetUniformLocation(program, "uViewProjection");
        albedoLocation = glGetUniformLocation(program, "uAlbedo");
        albedoTintLocation = glGetUniformLocation(program, "uAlbedoTint");
        ambientStrengthLocation = glGetUniformLocation(program, "uAmbientStrength");
        diffuseStrengthLocation = glGetUniformLocation(program, "uDiffuseStrength");
        uvScaleLocation = glGetUniformLocation(program, "uUvScale");
        if (!planetMaterial.valid()) return false;

        const auto sphere = jassim::render::makeUvSphere(1.25F, 40, 28);
        if (!sphere.valid() || sphere.indices.size() >
                static_cast<std::size_t>(std::numeric_limits<GLsizei>::max())) {
            __android_log_print(ANDROID_LOG_ERROR, kLogTag, "Procedural sphere mesh is invalid");
            return false;
        }
        indexCount = static_cast<GLsizei>(sphere.indices.size());

        glGenVertexArrays(1, &vertexArray);
        glGenBuffers(1, &vertexBuffer);
        glGenBuffers(1, &indexBuffer);
        glBindVertexArray(vertexArray);
        glBindBuffer(GL_ARRAY_BUFFER, vertexBuffer);
        glBufferData(GL_ARRAY_BUFFER,
                     static_cast<GLsizeiptr>(sphere.vertices.size() * sizeof(jassim::render::MeshVertex)),
                     sphere.vertices.data(), GL_STATIC_DRAW);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBuffer);
        glBufferData(GL_ELEMENT_ARRAY_BUFFER,
                     static_cast<GLsizeiptr>(sphere.indices.size() * sizeof(std::uint32_t)),
                     sphere.indices.data(), GL_STATIC_DRAW);
        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(jassim::render::MeshVertex),
                              reinterpret_cast<void*>(offsetof(jassim::render::MeshVertex, position)));
        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, sizeof(jassim::render::MeshVertex),
                              reinterpret_cast<void*>(offsetof(jassim::render::MeshVertex, normal)));
        glEnableVertexAttribArray(2);
        glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, sizeof(jassim::render::MeshVertex),
                              reinterpret_cast<void*>(offsetof(jassim::render::MeshVertex, uv)));
        glBindVertexArray(0);

        const auto planetTexture = jassim::render::makeProceduralPlanetAlbedo(256, 128);
        if (!planetTexture.valid()) return false;
        glGenTextures(1, &albedoTexture);
        glBindTexture(GL_TEXTURE_2D, albedoTexture);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
        glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB,
                     static_cast<GLsizei>(planetTexture.width),
                     static_cast<GLsizei>(planetTexture.height), 0,
                     GL_RGB, GL_UNSIGNED_BYTE, planetTexture.rgb.data());
        glGenerateMipmap(GL_TEXTURE_2D);
        glBindTexture(GL_TEXTURE_2D, 0);
        glEnable(GL_DEPTH_TEST);
        return glGetError() == GL_NO_ERROR;
    }

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
        camera.setPerspective(1.0471975512F,
                              height > 0 ? static_cast<float>(width) / height : 1.0F,
                              0.1F, 10000.0F);

        if (!createSceneResources()) {
            __android_log_print(ANDROID_LOG_ERROR, kLogTag, "Scene renderer initialization failed");
            shutdown();
            return false;
        }
        __android_log_print(ANDROID_LOG_INFO, kLogTag,
                            "GLES3 renderer ready: %dx%d", width, height);
        return true;
    }

    void draw() const {
        if (display == EGL_NO_DISPLAY || surface == EGL_NO_SURFACE) return;
        glViewport(0, 0, width, height);
        glClearColor(0.012F, 0.025F, 0.075F, 1.0F);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        if (program != 0 && vertexArray != 0) {
            const auto matrix = camera.viewProjection();
            glUseProgram(program);
            glUniformMatrix4fv(viewProjectionLocation, 1, GL_FALSE, matrix.values.data());
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, albedoTexture);
            glUniform1i(albedoLocation, 0);
            glUniform3f(albedoTintLocation, planetMaterial.albedoTint[0],
                        planetMaterial.albedoTint[1], planetMaterial.albedoTint[2]);
            glUniform1f(ambientStrengthLocation, planetMaterial.ambientStrength);
            glUniform1f(diffuseStrengthLocation, planetMaterial.diffuseStrength);
            glUniform1f(uvScaleLocation, planetMaterial.uvScale);
            glBindVertexArray(vertexArray);
            glDrawElements(GL_TRIANGLES, indexCount, GL_UNSIGNED_INT, nullptr);
            glBindVertexArray(0);
        }
        eglSwapBuffers(display, surface);
    }

    void shutdown() {
        if (display != EGL_NO_DISPLAY) {
            eglMakeCurrent(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
            if (albedoTexture) glDeleteTextures(1, &albedoTexture);
            if (indexBuffer) glDeleteBuffers(1, &indexBuffer);
            if (vertexBuffer) glDeleteBuffers(1, &vertexBuffer);
            if (vertexArray) glDeleteVertexArrays(1, &vertexArray);
            if (program) glDeleteProgram(program);
            albedoTexture = 0;
            indexBuffer = vertexBuffer = vertexArray = program = 0;
            indexCount = 0;
            viewProjectionLocation = albedoLocation = -1;
            albedoTintLocation = ambientStrengthLocation = diffuseStrengthLocation = uvScaleLocation = -1;
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
    } else if (command == APP_CMD_WINDOW_RESIZED && runtime->display != EGL_NO_DISPLAY) {
        eglQuerySurface(runtime->display, runtime->surface, EGL_WIDTH, &runtime->width);
        eglQuerySurface(runtime->display, runtime->surface, EGL_HEIGHT, &runtime->height);
        runtime->camera.setPerspective(1.0471975512F,
            runtime->height > 0 ? static_cast<float>(runtime->width) / runtime->height : 1.0F,
            0.1F, 10000.0F);
    } else if (command == APP_CMD_TERM_WINDOW) {
        runtime->shutdown();
        runtime->input.cancelAll();
    }
}

int32_t handleInput(android_app* app, AInputEvent* event) {
    auto* runtime = static_cast<AndroidRuntime*>(app->userData);
    if (runtime == nullptr || AInputEvent_getType(event) != AINPUT_EVENT_TYPE_MOTION) return 0;

    const int32_t action = AMotionEvent_getAction(event);
    const int32_t maskedAction = action & AMOTION_EVENT_ACTION_MASK;
    const std::size_t pointerCount = AMotionEvent_getPointerCount(event);

    if (maskedAction == AMOTION_EVENT_ACTION_CANCEL) {
        runtime->input.cancelAll();
        return 1;
    }

    if (maskedAction == AMOTION_EVENT_ACTION_DOWN ||
        maskedAction == AMOTION_EVENT_ACTION_POINTER_DOWN ||
        maskedAction == AMOTION_EVENT_ACTION_UP ||
        maskedAction == AMOTION_EVENT_ACTION_POINTER_UP) {
        const std::size_t index = static_cast<std::size_t>(
            (action & AMOTION_EVENT_ACTION_POINTER_INDEX_MASK) >>
            AMOTION_EVENT_ACTION_POINTER_INDEX_SHIFT);
        if (index >= pointerCount) return 0;
        const int32_t id = AMotionEvent_getPointerId(event, index);
        const float x = AMotionEvent_getX(event, index);
        const float y = AMotionEvent_getY(event, index);
        if (maskedAction == AMOTION_EVENT_ACTION_DOWN ||
            maskedAction == AMOTION_EVENT_ACTION_POINTER_DOWN) {
            runtime->input.touchDown(id, x, y);
        } else {
            runtime->input.touchUp(id, x, y);
        }
        return 1;
    }

    if (maskedAction == AMOTION_EVENT_ACTION_MOVE) {
        for (std::size_t i = 0; i < pointerCount; ++i) {
            runtime->input.touchMove(AMotionEvent_getPointerId(event, i),
                                     AMotionEvent_getX(event, i),
                                     AMotionEvent_getY(event, i));
        }
        return 1;
    }

    return 0;
}

} // namespace

void android_main(android_app* app) {
    app_dummy();
    AndroidRuntime runtime;
    app->userData = &runtime;
    app->onAppCmd = handleCommand;
    app->onInputEvent = handleInput;

    while (!app->destroyRequested) {
        android_poll_source* source = nullptr;
        const int events = ALooper_pollOnce(16, nullptr, nullptr,
                                            reinterpret_cast<void**>(&source));
        if (events >= 0 && source != nullptr) source->process(app, source);
        runtime.draw();
        runtime.input.clearFrame();
    }

    app->userData = nullptr;
}
