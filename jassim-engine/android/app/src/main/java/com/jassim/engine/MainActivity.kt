package com.jassim.engine

import android.app.Activity
import android.os.Bundle

class MainActivity : Activity() {
    private lateinit var engineView: EngineSurfaceView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        engineView = EngineSurfaceView(this)
        setContentView(engineView)
    }

    override fun onPause() {
        engineView.onPause()
        super.onPause()
    }

    override fun onResume() {
        super.onResume()
        engineView.onResume()
    }
}
