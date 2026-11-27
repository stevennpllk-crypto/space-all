package com.jassim.engine

import android.content.Context
import android.opengl.GLSurfaceView
import android.view.MotionEvent
import android.view.ScaleGestureDetector

class EngineSurfaceView(context: Context) : GLSurfaceView(context) {
    private val engineRenderer = EngineRenderer()
    private var previousX = 0.0f
    private var previousY = 0.0f
    private val scaleDetector = ScaleGestureDetector(context,
        object : ScaleGestureDetector.SimpleOnScaleGestureListener() {
            override fun onScale(detector: ScaleGestureDetector): Boolean {
                engineRenderer.zoomBy(detector.scaleFactor)
                return true
            }
        })

    init {
        setEGLContextClientVersion(3)
        preserveEGLContextOnPause = true
        setRenderer(engineRenderer)
        renderMode = RENDERMODE_CONTINUOUSLY
    }

    override fun onTouchEvent(event: MotionEvent): Boolean {
        scaleDetector.onTouchEvent(event)
        when (event.actionMasked) {
            MotionEvent.ACTION_DOWN, MotionEvent.ACTION_POINTER_DOWN -> {
                previousX = event.getX(0)
                previousY = event.getY(0)
            }
            MotionEvent.ACTION_POINTER_UP -> {
                val remainingPointer = if (event.actionIndex == 0) 1 else 0
                if (remainingPointer < event.pointerCount) {
                    previousX = event.getX(remainingPointer)
                    previousY = event.getY(remainingPointer)
                }
            }
            MotionEvent.ACTION_MOVE -> {
                if (event.pointerCount == 1 && !scaleDetector.isInProgress) {
                    val x = event.x
                    val y = event.y
                    engineRenderer.rotateBy((x - previousX) * 0.45f, (y - previousY) * 0.45f)
                    previousX = x
                    previousY = y
                }
            }
            MotionEvent.ACTION_UP, MotionEvent.ACTION_CANCEL -> performClick()
        }
        return true
    }

    override fun performClick(): Boolean {
        super.performClick()
        return true
    }
}
