package com.jassim.engine

import org.junit.Assert.assertEquals
import org.junit.Test

class EngineCameraTest {
    @Test
    fun startsAtStableDefaultPose() {
        val camera = EngineCamera()
        assertEquals(-18.0f, camera.rotationX, 0.0001f)
        assertEquals(0.0f, camera.rotationY, 0.0001f)
        assertEquals(2.8f, camera.distance, 0.0001f)
    }

    @Test
    fun pitchIsClampedToAvoidFlipping() {
        val camera = EngineCamera()
        camera.rotateBy(0.0f, 500.0f)
        assertEquals(85.0f, camera.rotationX, 0.0001f)
        camera.rotateBy(0.0f, -1000.0f)
        assertEquals(-85.0f, camera.rotationX, 0.0001f)
    }

    @Test
    fun yawWrapsWithoutGrowingUnbounded() {
        val camera = EngineCamera()
        camera.rotateBy(810.0f, 0.0f)
        assertEquals(90.0f, camera.rotationY, 0.0001f)
        camera.rotateBy(-180.0f, 0.0f)
        assertEquals(270.0f, camera.rotationY, 0.0001f)
    }

    @Test
    fun zoomStaysWithinSafeDistanceRange() {
        val camera = EngineCamera()
        camera.zoomBy(100.0f)
        assertEquals(EngineCamera.MIN_DISTANCE, camera.distance, 0.0001f)
        camera.zoomBy(0.001f)
        assertEquals(EngineCamera.MAX_DISTANCE, camera.distance, 0.0001f)
    }

    @Test
    fun invalidGestureValuesAreIgnored() {
        val camera = EngineCamera()
        camera.rotateBy(Float.NaN, Float.POSITIVE_INFINITY)
        camera.zoomBy(Float.NaN)
        camera.zoomBy(-2.0f)
        assertEquals(-18.0f, camera.rotationX, 0.0001f)
        assertEquals(0.0f, camera.rotationY, 0.0001f)
        assertEquals(2.8f, camera.distance, 0.0001f)
    }
}
