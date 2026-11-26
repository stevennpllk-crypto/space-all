#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspctrl.h>
#include <pspgu.h>
#include <math.h>
#include <stdlib.h>

PSP_MODULE_INFO("SOLAR EXPLORER", 0, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);

#define SCREEN_W 480
#define SCREEN_H 272
#define PI 3.14159265358979323846f
#define STAR_COUNT 90

static unsigned int __attribute__((aligned(16))) display_list[262144];

typedef struct {
    unsigned int color;
    float x, y, z;
} Vertex;

static Vertex stars[STAR_COUNT];
static float marker_x = 240.0f;
static float marker_y = 136.0f;
static float zoom = 1.0f;
static float orbit_angle = 0.0f;

static void draw_rect(float x, float y, float w, float h, unsigned int color) {
    Vertex v[2] = {
        { color, x, y, 0.0f },
        { color, x + w, y + h, 0.0f }
    };
    sceGuDrawArray(GU_SPRITES,
        GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_2D,
        2, 0, v);
}

static void draw_circle(float cx, float cy, float radius, unsigned int color) {
    Vertex v[34];
    v[0].color = color;
    v[0].x = cx;
    v[0].y = cy;
    v[0].z = 0.0f;

    for (int i = 0; i <= 32; ++i) {
        float a = (float)i * 2.0f * PI / 32.0f;
        v[i + 1].color = color;
        v[i + 1].x = cx + cosf(a) * radius;
        v[i + 1].y = cy + sinf(a) * radius;
        v[i + 1].z = 0.0f;
    }

    sceGuDrawArray(GU_TRIANGLE_FAN,
        GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_2D,
        34, 0, v);
}

static void init_stars(void) {
    srand(20261126);
    for (int i = 0; i < STAR_COUNT; ++i) {
        stars[i].color = (i % 6 == 0) ? 0xFFFFFFFF : 0xFF91A9C9;
        stars[i].x = (float)(rand() % SCREEN_W);
        stars[i].y = (float)(rand() % SCREEN_H);
        stars[i].z = 0.0f;
    }
}

static void init_graphics(void) {
    sceGuInit();
    sceGuStart(GU_DIRECT, display_list);
    sceGuDrawBuffer(GU_PSM_8888, (void *)0, 512);
    sceGuDispBuffer(SCREEN_W, SCREEN_H, (void *)0x88000, 512);
    sceGuDepthBuffer((void *)0x110000, 512);
    sceGuOffset(2048 - SCREEN_W / 2, 2048 - SCREEN_H / 2);
    sceGuViewport(2048, 2048, SCREEN_W, SCREEN_H);
    sceGuScissor(0, 0, SCREEN_W, SCREEN_H);
    sceGuEnable(GU_SCISSOR_TEST);
    sceGuDisable(GU_DEPTH_TEST);
    sceGuDisable(GU_CULL_FACE);
    sceGuClearColor(0xFF030612);
    sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
    sceGuFinish();
    sceGuSync(0, 0);
    sceDisplayWaitVblankStart();
    sceGuDisplay(GU_TRUE);
}

static void draw_frame(void) {
    sceGuStart(GU_DIRECT, display_list);
    sceGuClearColor(0xFF030612);
    sceGuClear(GU_COLOR_BUFFER_BIT);

    for (int i = 0; i < STAR_COUNT; ++i) {
        draw_rect(stars[i].x, stars[i].y, 1.5f, 1.5f, stars[i].color);
        stars[i].x -= 0.10f;
        if (stars[i].x < 0.0f) stars[i].x = SCREEN_W;
    }

    for (int ring = 0; ring < 4; ++ring) {
        float rx = (48.0f + (float)ring * 28.0f) * zoom;
        float ry = rx * 0.55f;
        Vertex line[65];
        for (int j = 0; j <= 64; ++j) {
            float a = (float)j * 2.0f * PI / 64.0f;
            line[j].color = 0xFF35445D;
            line[j].x = 240.0f + cosf(a) * rx;
            line[j].y = 136.0f + sinf(a) * ry;
            line[j].z = 0.0f;
        }
        sceGuDrawArray(GU_LINE_STRIP,
            GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_2D,
            65, 0, line);
    }

    draw_circle(240.0f, 136.0f, 29.0f * zoom, 0x303A72C8);
    draw_circle(240.0f, 136.0f, 21.0f * zoom, 0xFF168DFF);
    draw_circle(235.0f, 130.0f, 9.0f * zoom, 0xFF75D7FF);

    const unsigned int colors[4] = {
        0xFFB9A17D, 0xFF55A6D8, 0xFFB85C42, 0xFFBDAA83
    };
    const float sizes[4] = { 3.5f, 6.0f, 4.5f, 10.0f };

    for (int i = 0; i < 4; ++i) {
        float a = orbit_angle * (0.7f / (float)(i + 1)) + (float)i * 1.7f;
        float orbit = (48.0f + (float)i * 28.0f) * zoom;
        float px = 240.0f + cosf(a) * orbit;
        float py = 136.0f + sinf(a) * orbit * 0.55f;
        draw_circle(px, py, sizes[i] * zoom, colors[i]);
        if (i == 1) draw_circle(px, py, sizes[i] * zoom + 1.5f, 0x8879C9E8);
    }

    draw_circle(marker_x, marker_y, 8.0f, 0xAA35D9FF);
    draw_circle(marker_x, marker_y, 3.0f, 0xFFFFFFFF);
    draw_rect(10.0f, 10.0f, 100.0f, 2.0f, 0xFF35B8E8);
    draw_rect(10.0f, 16.0f, 62.0f, 2.0f, 0xFF7189B5);

    sceGuFinish();
    sceGuSync(0, 0);
    sceDisplayWaitVblankStart();
    sceGuSwapBuffers();
    orbit_angle += 0.006f;
}

int main(void) {
    sceCtrlSetSamplingCycle(0);
    sceCtrlSetSamplingMode(PSP_CTRL_MODE_DIGITAL);
    init_stars();
    init_graphics();

    while (1) {
        SceCtrlData pad;
        sceCtrlReadBufferPositive(&pad, 1);

        if (pad.Buttons & PSP_CTRL_START) break;
        if (pad.Buttons & PSP_CTRL_LEFT) marker_x -= 2.0f;
        if (pad.Buttons & PSP_CTRL_RIGHT) marker_x += 2.0f;
        if (pad.Buttons & PSP_CTRL_UP) marker_y -= 2.0f;
        if (pad.Buttons & PSP_CTRL_DOWN) marker_y += 2.0f;
        if (pad.Buttons & PSP_CTRL_LTRIGGER) zoom -= 0.008f;
        if (pad.Buttons & PSP_CTRL_RTRIGGER) zoom += 0.008f;

        if (zoom < 0.55f) zoom = 0.55f;
        if (zoom > 1.70f) zoom = 1.70f;
        if (marker_x < 5.0f) marker_x = 5.0f;
        if (marker_x > 475.0f) marker_x = 475.0f;
        if (marker_y < 5.0f) marker_y = 5.0f;
        if (marker_y > 267.0f) marker_y = 267.0f;

        draw_frame();
    }

    sceGuTerm();
    sceKernelExitGame();
    return 0;
}
