package com.jassim.engine

import android.content.Context
import android.opengl.GLSurfaceView

class EngineSurfaceView(context: Context) : GLSurfaceView(context) {
    init {
        setEGLContextClientVersion(3)
        preserveEGLContextOnPause = true
        setRenderer(EngineRenderer())
        renderMode = RENDERMODE_CONTINUOUSLY
    }
}
