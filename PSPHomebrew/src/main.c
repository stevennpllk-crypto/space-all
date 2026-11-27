#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspctrl.h>
#include <pspgu.h>
#include <pspgum.h>
#include <math.h>
#include <stdlib.h>
#include <string.h>

PSP_MODULE_INFO("SOLAR EXPLORER", 0, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);

#define SW 480
#define SH 272
#define PI 3.14159265358979323846f
#define STAR_COUNT 84
#define SPHERE_STACKS 9
#define SPHERE_SLICES 14
#define SPHERE_VERTS (SPHERE_STACKS * SPHERE_SLICES * 6)

static unsigned int __attribute__((aligned(16))) display_list[262144];
typedef struct { unsigned int color; float x, y, z; } Vertex;
static Vertex stars[STAR_COUNT];
static Vertex sphere_mesh[SPHERE_VERTS];
static float ship_x = 0.0f, ship_z = 13.0f, ship_yaw = 0.0f;
static float orbit_time = 0.0f, camera_distance = 34.0f;
static int selected_planet = 2;
static int mission_mode = 0;
static int language = 0; /* 0 English, 1 Arabic UI assets planned */

static const float orbit_radius[8] = {3.3f, 5.0f, 7.1f, 9.3f, 13.0f, 16.5f, 20.0f, 23.5f};
static const float planet_size[8] = {0.34f, 0.58f, 0.62f, 0.46f, 1.45f, 1.18f, 0.92f, 0.88f};
static const unsigned int planet_color[8] = {
    0xFF9B8D78, 0xFF70B6D9, 0xFF4C9A55, 0xFFB65C45,
    0xFFC69B6A, 0xFFD5C18D, 0xFF86C4C9, 0xFF456CB4
};
static const float orbit_speed[8] = {1.55f, 1.12f, 0.82f, 0.66f, 0.36f, 0.25f, 0.18f, 0.13f};

static unsigned int shade(unsigned int base, float light) {
    int r = (int)((base & 255) * light);
    int g = (int)(((base >> 8) & 255) * light);
    int b = (int)(((base >> 16) & 255) * light);
    if (r > 255) r = 255; if (g > 255) g = 255; if (b > 255) b = 255;
    return 0xFF000000u | ((unsigned int)b << 16) | ((unsigned int)g << 8) | (unsigned int)r;
}

static void build_sphere(unsigned int base) {
    int n = 0;
    const float lx = -0.45f, ly = 0.35f, lz = 0.82f;
    for (int lat = 0; lat < SPHERE_STACKS; ++lat) {
        float p0 = -PI * 0.5f + PI * (float)lat / (float)SPHERE_STACKS;
        float p1 = -PI * 0.5f + PI * (float)(lat + 1) / (float)SPHERE_STACKS;
        for (int lon = 0; lon < SPHERE_SLICES; ++lon) {
            float t0 = 2.0f * PI * (float)lon / (float)SPHERE_SLICES;
            float t1 = 2.0f * PI * (float)(lon + 1) / (float)SPHERE_SLICES;
            float pp[4] = {p0,p0,p1,p1};
            float tt[4] = {t0,t1,t0,t1};
            float xyz[4][3];
            for (int k=0;k<4;k++) {
                xyz[k][0] = cosf(pp[k]) * cosf(tt[k]);
                xyz[k][1] = sinf(pp[k]);
                xyz[k][2] = cosf(pp[k]) * sinf(tt[k]);
            }
            const int order[6] = {0,2,1,1,2,3};
            for (int k=0;k<6;k++) {
                int q=order[k];
                float d=xyz[q][0]*lx + xyz[q][1]*ly + xyz[q][2]*lz;
                float light=0.24f + 0.76f * (d>0.0f?d:0.0f);
                sphere_mesh[n].color=shade(base,light);
                sphere_mesh[n].x=xyz[q][0];
                sphere_mesh[n].y=xyz[q][1];
                sphere_mesh[n].z=xyz[q][2];
                n++;
            }
        }
    }
}

static void draw_sphere(float x,float y,float z,float radius,unsigned int color,float spin) {
    build_sphere(color);
    sceGumPushMatrix();
    ScePspFVector3 pos={x,y,z};
    sceGumTranslate(&pos);
    sceGumRotateY(spin);
    ScePspFVector3 scale={radius,radius,radius};
    sceGumScale(&scale);
    sceGumDrawArray(GU_TRIANGLES,
        GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,
        SPHERE_VERTS,0,sphere_mesh);
    sceGumPopMatrix();
}

static void draw_orbit(float radius) {
    Vertex line[49];
    for(int i=0;i<49;i++) {
        float a=2.0f*PI*(float)i/48.0f;
        line[i].color=0xFF293B55;
        line[i].x=cosf(a)*radius;
        line[i].y=0.0f;
        line[i].z=sinf(a)*radius;
    }
    sceGumDrawArray(GU_LINE_STRIP,
        GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,49,0,line);
}

static void draw_ship(void) {
    /* Low-poly exploration craft, deliberately small to preserve fill rate. */
    static const Vertex craft[] = {
        {0xFFE5F5FF,0.0f,0.0f,-1.35f},
        {0xFF58BCEB,-0.72f,-0.18f,0.72f},
        {0xFF58BCEB,0.72f,-0.18f,0.72f},
        {0xFFB5DDF0,0.0f,0.28f,0.55f},
        {0xFF2C6F9A,-0.72f,-0.18f,0.72f},
        {0xFF2C6F9A,0.0f,-0.55f,0.65f},
        {0xFF2C6F9A,0.72f,-0.18f,0.72f},
        {0xFFFF9B42,-0.22f,-0.15f,0.75f},
        {0xFFFFD36A,0.0f,-0.18f,1.28f},
        {0xFFFF9B42,0.22f,-0.15f,0.75f}
    };
    sceGumPushMatrix();
    ScePspFVector3 pos={ship_x,0.15f,ship_z};
    sceGumTranslate(&pos);
    sceGumRotateY(ship_yaw);
    ScePspFVector3 scale={0.62f,0.62f,0.62f};
    sceGumScale(&scale);
    sceGumDrawArray(GU_TRIANGLES,
        GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,
        9,0,craft);
    sceGumPopMatrix();
}

static void init_stars(void) {
    srand(20261127);
    for(int i=0;i<STAR_COUNT;i++) {
        stars[i].color=(i%5==0)?0xFFFFFFFF:0xFF8299B9;
        stars[i].x=(float)(rand()%SW);
        stars[i].y=(float)(rand()%SH);
        stars[i].z=0.0f;
    }
}

static void init_graphics(void) {
    sceGuInit();
    sceGuStart(GU_DIRECT,display_list);
    sceGuDrawBuffer(GU_PSM_8888,(void*)0,512);
    sceGuDispBuffer(SW,SH,(void*)0x88000,512);
    sceGuDepthBuffer((void*)0x110000,512);
    sceGuOffset(2048-SW/2,2048-SH/2);
    sceGuViewport(2048,2048,SW,SH);
    sceGuScissor(0,0,SW,SH);
    sceGuEnable(GU_SCISSOR_TEST);
    sceGuEnable(GU_DEPTH_TEST);
    sceGuDepthRange(65535,0);
    sceGuDepthFunc(GU_GEQUAL);
    sceGuDisable(GU_CULL_FACE);
    sceGuShadeModel(GU_SMOOTH);
    sceGuClearColor(0xFF030612);
    sceGuClearDepth(0);
    sceGuClear(GU_COLOR_BUFFER_BIT|GU_DEPTH_BUFFER_BIT);
    sceGuFinish();
    sceGuSync(0,0);
    sceDisplayWaitVblankStart();
    sceGuDisplay(GU_TRUE);
}

static void draw_starfield(void) {
    sceGuDisable(GU_DEPTH_TEST);
    for(int i=0;i<STAR_COUNT;i++) {
        Vertex dot[2]={{stars[i].color,stars[i].x,stars[i].y,0.0f},
                       {stars[i].color,stars[i].x+1.2f,stars[i].y+1.2f,0.0f}};
        sceGuDrawArray(GU_SPRITES,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_2D,2,0,dot);
        stars[i].x-=0.035f;
        if(stars[i].x<0.0f) stars[i].x=SW;
    }
    sceGuEnable(GU_DEPTH_TEST);
}

static void draw_scene(void) {
    sceGuStart(GU_DIRECT,display_list);
    sceGuClearColor(0xFF030612);
    sceGuClearDepth(0);
    sceGuClear(GU_COLOR_BUFFER_BIT|GU_DEPTH_BUFFER_BIT);
    draw_starfield();

    sceGumMatrixMode(GU_PROJECTION);
    sceGumLoadIdentity();
    sceGumPerspective(48.0f,SW/(float)SH,0.5f,90.0f);

    sceGumMatrixMode(GU_VIEW);
    sceGumLoadIdentity();
    ScePspFVector3 eye={0.0f,10.0f,camera_distance};
    ScePspFVector3 target={0.0f,0.0f,0.0f};
    ScePspFVector3 up={0.0f,1.0f,0.0f};
    sceGumLookAt(&eye,&target,&up);

    sceGumMatrixMode(GU_MODEL);
    sceGumLoadIdentity();

    for(int i=0;i<8;i++) {
        sceGumPushMatrix();
        draw_orbit(orbit_radius[i]);
        sceGumPopMatrix();
    }

    /* Sun */
    draw_sphere(0.0f,0.0f,0.0f,1.75f,0xFF3D9DF4,orbit_time*0.08f);

    /* Eight planets on compressed, readable orbital distances. */
    for(int i=0;i<8;i++) {
        float a=orbit_time*orbit_speed[i]+(float)i*0.71f;
        float x=cosf(a)*orbit_radius[i];
        float z=sinf(a)*orbit_radius[i];
        draw_sphere(x,0.0f,z,planet_size[i],planet_color[i],orbit_time*0.18f+i);
        if(i==selected_planet) {
            Vertex target_ring[33];
            float rr=planet_size[i]+0.48f;
            for(int j=0;j<33;j++) {
                float t=2.0f*PI*(float)j/32.0f;
                target_ring[j].color=mission_mode?0xFFFFB34D:0xFF65D9FF;
                target_ring[j].x=x+cosf(t)*rr;
                target_ring[j].y=0.10f;
                target_ring[j].z=z+sinf(t)*rr;
            }
            sceGuDrawArray(GU_LINE_STRIP,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,33,0,target_ring);
            if(mission_mode) {
                Vertex route[2]={{0xFFFFB34D,ship_x,0.12f,ship_z},{0xFFFFB34D,x,0.12f,z}};
                sceGuDrawArray(GU_LINES,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,2,0,route);
            }
        }
        if(i==5) {
            /* Saturn-like ring, shown as a thin elliptical wire orbit. */
            Vertex ring[49];
            for(int j=0;j<49;j++) {
                float t=2.0f*PI*(float)j/48.0f;
                ring[j].color=0xFFB7A77E;
                ring[j].x=x+cosf(t)*1.85f;
                ring[j].y=0.12f;
                ring[j].z=z+sinf(t)*0.72f;
            }
            sceGuDrawArray(GU_LINE_STRIP,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,49,0,ring);
        }
    }

    draw_ship();

    /* Minimal HUD bars; text/localized glyph atlas is a separate next milestone. */
    sceGuDisable(GU_DEPTH_TEST);
    Vertex hud1[2]={{0xFF37BDEB,10,10,0},{0xFF37BDEB,124,13,0}};
    Vertex hud2[2]={{0xFF879CB8,10,18,0},{0xFF879CB8,78,21,0}};
    sceGuDrawArray(GU_SPRITES,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_2D,2,0,hud1);
    sceGuDrawArray(GU_SPRITES,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_2D,2,0,hud2);
    sceGuEnable(GU_DEPTH_TEST);

    sceGuFinish();
    sceGuSync(0,0);
    sceDisplayWaitVblankStart();
    sceGuSwapBuffers();
    orbit_time+=0.0045f;
}

int main(void) {
    sceCtrlSetSamplingCycle(0);
    sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);
    init_stars();
    init_graphics();

    while(1) {
        SceCtrlData pad;
        sceCtrlReadBufferPositive(&pad,1);
        if(pad.Buttons&PSP_CTRL_START) break;

        float turn=(pad.Buttons&PSP_CTRL_LTRIGGER)?0.045f:0.025f;
        float thrust=(pad.Buttons&PSP_CTRL_RTRIGGER)?0.16f:0.075f;
        if(pad.Buttons&PSP_CTRL_LEFT) ship_yaw-=turn;
        if(pad.Buttons&PSP_CTRL_RIGHT) ship_yaw+=turn;
        if(pad.Buttons&PSP_CTRL_UP) {
            ship_x+=sinf(ship_yaw)*thrust;
            ship_z-=cosf(ship_yaw)*thrust;
        }
        if(pad.Buttons&PSP_CTRL_DOWN) {
            ship_x-=sinf(ship_yaw)*thrust*0.65f;
            ship_z+=cosf(ship_yaw)*thrust*0.65f;
        }
        if(pad.Buttons&PSP_CTRL_TRIANGLE) mission_mode=!mission_mode;
        if(pad.Buttons&PSP_CTRL_CROSS) selected_planet=(selected_planet+1)%8;
        if(pad.Buttons&PSP_CTRL_CIRCLE) language=!language;
        if(pad.Buttons&PSP_CTRL_LTRIGGER) camera_distance-=0.035f;
        if(pad.Buttons&PSP_CTRL_RTRIGGER) camera_distance+=0.035f;
        if(camera_distance<22.0f) camera_distance=22.0f;
        if(camera_distance>48.0f) camera_distance=48.0f;
        if(ship_x<-26.0f) ship_x=-26.0f;
        if(ship_x>26.0f) ship_x=26.0f;
        if(ship_z<-26.0f) ship_z=-26.0f;
        if(ship_z>26.0f) ship_z=26.0f;

        /* Mission/exploration state and language selection are kept in runtime state.
           On-screen bilingual typography is added with the font atlas milestone. */
        (void)mission_mode;
        (void)selected_planet;
        (void)language;
        draw_scene();
    }

    sceGuTerm();
    sceKernelExitGame();
    return 0;
}
