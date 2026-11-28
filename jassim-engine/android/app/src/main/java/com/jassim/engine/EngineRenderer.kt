package com.jassim.engine

import android.opengl.GLES30
import android.opengl.GLSurfaceView
import android.opengl.Matrix
import android.util.Log
import java.nio.ByteBuffer
import java.nio.ByteOrder
import java.nio.FloatBuffer
import java.nio.ShortBuffer
import javax.microedition.khronos.egl.EGLConfig
import javax.microedition.khronos.opengles.GL10

class EngineRenderer : GLSurfaceView.Renderer {
    private var program = 0
    private var vertexBufferId = 0
    private var indexBufferId = 0
    private var mvpLocation = -1
    private var modelLocation = -1
    private var surfaceWidth = 1
    private var surfaceHeight = 1
    private val startTime = System.nanoTime()
    private val camera = EngineCamera()

    fun rotateBy(deltaX: Float, deltaY: Float) = camera.rotateBy(deltaX, deltaY)

    fun zoomBy(scaleFactor: Float) = camera.zoomBy(scaleFactor)

    // Each face owns four vertices so its material color stays independent.
    // Vertex layout: position (xyz), face color (rgb).
    private val vertices = floatArrayOf(
        // Front (+Z): blue
        -0.6f, -0.6f,  0.6f, 0.18f, 0.55f, 1.00f,
         0.6f, -0.6f,  0.6f, 0.18f, 0.55f, 1.00f,
         0.6f,  0.6f,  0.6f, 0.18f, 0.55f, 1.00f,
        -0.6f,  0.6f,  0.6f, 0.18f, 0.55f, 1.00f,
        // Back (-Z): deep blue
         0.6f, -0.6f, -0.6f, 0.08f, 0.22f, 0.55f,
        -0.6f, -0.6f, -0.6f, 0.08f, 0.22f, 0.55f,
        -0.6f,  0.6f, -0.6f, 0.08f, 0.22f, 0.55f,
         0.6f,  0.6f, -0.6f, 0.08f, 0.22f, 0.55f,
        // Left (-X): cyan
        -0.6f, -0.6f, -0.6f, 0.12f, 0.72f, 0.88f,
        -0.6f, -0.6f,  0.6f, 0.12f, 0.72f, 0.88f,
        -0.6f,  0.6f,  0.6f, 0.12f, 0.72f, 0.88f,
        -0.6f,  0.6f, -0.6f, 0.12f, 0.72f, 0.88f,
        // Right (+X): violet
         0.6f, -0.6f,  0.6f, 0.48f, 0.30f, 0.92f,
         0.6f, -0.6f, -0.6f, 0.48f, 0.30f, 0.92f,
         0.6f,  0.6f, -0.6f, 0.48f, 0.30f, 0.92f,
         0.6f,  0.6f,  0.6f, 0.48f, 0.30f, 0.92f,
        // Top (+Y): light blue
        -0.6f,  0.6f,  0.6f, 0.48f, 0.76f, 1.00f,
         0.6f,  0.6f,  0.6f, 0.48f, 0.76f, 1.00f,
         0.6f,  0.6f, -0.6f, 0.48f, 0.76f, 1.00f,
        -0.6f,  0.6f, -0.6f, 0.48f, 0.76f, 1.00f,
        // Bottom (+/-Y): dark indigo
        -0.6f, -0.6f, -0.6f, 0.10f, 0.16f, 0.38f,
         0.6f, -0.6f, -0.6f, 0.10f, 0.16f, 0.38f,
         0.6f, -0.6f,  0.6f, 0.10f, 0.16f, 0.38f,
        -0.6f, -0.6f,  0.6f, 0.10f, 0.16f, 0.38f
    )

    private val indices = shortArrayOf(
         0,  1,  2,  0,  2,  3,
         4,  5,  6,  4,  6,  7,
         8,  9, 10,  8, 10, 11,
        12, 13, 14, 12, 14, 15,
        16, 17, 18, 16, 18, 19,
        20, 21, 22, 20, 22, 23
    )

    // The renderer consumes the validated CPU mesh; GPU buffers are created only
    // on the GL thread in onSurfaceCreated().
    private val mesh = EngineMesh(
        positions = FloatArray(vertices.size / 2) { i -> vertices[(i / 3) * 6 + (i % 3)] },
        colors = FloatArray(vertices.size / 2) { i -> vertices[(i / 3) * 6 + 3 + (i % 3)] },
        indices = IntArray(indices.size) { i -> indices[i].toInt() and 0xffff }
    )

    private val meshIndexCount = mesh.indices.size

    private val projection = FloatArray(16)
    private val view = FloatArray(16)
    private val model = FloatArray(16)
    private val viewModel = FloatArray(16)
    private val mvp = FloatArray(16)

    override fun onSurfaceCreated(gl: GL10?, config: EGLConfig?) {
        GLES30.glClearColor(0.035f, 0.055f, 0.12f, 1.0f)
        GLES30.glEnable(GLES30.GL_DEPTH_TEST)
        GLES30.glDepthFunc(GLES30.GL_LEQUAL)
        GLES30.glEnable(GLES30.GL_CULL_FACE)
        GLES30.glCullFace(GLES30.GL_BACK)
        GLES30.glFrontFace(GLES30.GL_CCW)
        Log.i("JASSIM_ENGINE", "${NativeBridge.nativeEngineVersion()} rootNode=${NativeBridge.nativeCreateRootNode()}")

        val vertexShader = compileShader(GLES30.GL_VERTEX_SHADER,
            "#version 300 es\nlayout(location=0) in vec3 aPosition;\nlayout(location=1) in vec3 aColor;\nuniform mat4 uMvp;\nuniform mat4 uModel;\nout vec3 vColor;\nout vec3 vWorldPosition;\nvoid main(){ vec4 world=uModel*vec4(aPosition,1.0); vWorldPosition=world.xyz; vColor=aColor; gl_Position=uMvp*vec4(aPosition,1.0); }")
        val fragmentShader = compileShader(GLES30.GL_FRAGMENT_SHADER,
            "#version 300 es\nprecision highp float;\nin vec3 vColor;\nin vec3 vWorldPosition;\nout vec4 outColor;\nvoid main(){ vec3 N=normalize(cross(dFdx(vWorldPosition),dFdy(vWorldPosition))); if(!gl_FrontFacing) N=-N; vec3 L=normalize(vec3(-0.45,0.75,0.55)); vec3 V=normalize(vec3(0.0,0.0,3.0)-vWorldPosition); vec3 H=normalize(L+V); float diffuse=max(dot(N,L),0.0); float specular=pow(max(dot(N,H),0.0),48.0)*0.32; float rim=pow(1.0-max(dot(N,V),0.0),3.0)*0.10; vec3 linearColor=vColor*(0.16+diffuse*0.92)+vec3(specular+rim); vec3 mapped=linearColor/(linearColor+vec3(0.65)); mapped=pow(mapped,vec3(1.0/2.2)); outColor=vec4(mapped,1.0); }")
        program = GLES30.glCreateProgram()
        GLES30.glAttachShader(program, vertexShader)
        GLES30.glAttachShader(program, fragmentShader)
        GLES30.glLinkProgram(program)
        val status = IntArray(1)
        GLES30.glGetProgramiv(program, GLES30.GL_LINK_STATUS, status, 0)
        if (status[0] == 0) {
            val log = GLES30.glGetProgramInfoLog(program)
            GLES30.glDeleteProgram(program)
            program = 0
            throw IllegalStateException("JASSIM shader link failed: $log")
        }
        GLES30.glDeleteShader(vertexShader)
        GLES30.glDeleteShader(fragmentShader)
        mvpLocation = GLES30.glGetUniformLocation(program, "uMvp")
        modelLocation = GLES30.glGetUniformLocation(program, "uModel")

        val meshPositions = mesh.positions
        val meshColors = mesh.colors
        val interleavedVertices = FloatArray(mesh.vertexCount * 6) { i ->
            val vertex = i / 6
            val component = i % 6
            if (component < 3) meshPositions[vertex * 3 + component]
            else meshColors[vertex * 3 + component - 3]
        }
        val meshIndices = mesh.indices
        val gpuIndices = ShortArray(meshIndices.size) { i -> meshIndices[i].toShort() }
        val vertexData: FloatBuffer = ByteBuffer.allocateDirect(interleavedVertices.size * 4)
            .order(ByteOrder.nativeOrder()).asFloatBuffer().apply { put(interleavedVertices); position(0) }
        val indexData: ShortBuffer = ByteBuffer.allocateDirect(gpuIndices.size * 2)
            .order(ByteOrder.nativeOrder()).asShortBuffer().apply { put(gpuIndices); position(0) }

        val buffers = IntArray(2)
        GLES30.glGenBuffers(2, buffers, 0)
        vertexBufferId = buffers[0]
        indexBufferId = buffers[1]
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, vertexBufferId)
        GLES30.glBufferData(GLES30.GL_ARRAY_BUFFER, interleavedVertices.size * 4, vertexData, GLES30.GL_STATIC_DRAW)
        GLES30.glBindBuffer(GLES30.GL_ELEMENT_ARRAY_BUFFER, indexBufferId)
        GLES30.glBufferData(GLES30.GL_ELEMENT_ARRAY_BUFFER, gpuIndices.size * 2, indexData, GLES30.GL_STATIC_DRAW)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, 0)
        GLES30.glBindBuffer(GLES30.GL_ELEMENT_ARRAY_BUFFER, 0)
    }

    override fun onSurfaceChanged(gl: GL10?, width: Int, height: Int) {
        surfaceWidth = width.coerceAtLeast(1)
        surfaceHeight = height.coerceAtLeast(1)
        GLES30.glViewport(0, 0, surfaceWidth, surfaceHeight)
        val aspect = surfaceWidth.toFloat() / surfaceHeight.toFloat()
        Matrix.perspectiveM(projection, 0, 52.0f, aspect, 0.1f, 10.0f)
    }

    override fun onDrawFrame(gl: GL10?) {
        GLES30.glClear(GLES30.GL_COLOR_BUFFER_BIT or GLES30.GL_DEPTH_BUFFER_BIT)
        if (program == 0 || vertexBufferId == 0 || indexBufferId == 0) return

        val elapsedSeconds = (System.nanoTime() - startTime) / 1_000_000_000.0f
        val cameraDistance = camera.distance
        val rotationX = camera.rotationX
        val rotationY = camera.rotationY
        Matrix.setLookAtM(view, 0, 0.0f, 0.0f, cameraDistance, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f)
        Matrix.setIdentityM(model, 0)
        Matrix.rotateM(model, 0, rotationY + elapsedSeconds * 18.0f, 0.0f, 1.0f, 0.0f)
        Matrix.rotateM(model, 0, rotationX, 1.0f, 0.0f, 0.0f)
        Matrix.multiplyMM(viewModel, 0, view, 0, model, 0)
        Matrix.multiplyMM(mvp, 0, projection, 0, viewModel, 0)

        GLES30.glUseProgram(program)
        GLES30.glUniformMatrix4fv(mvpLocation, 1, false, mvp, 0)
        GLES30.glUniformMatrix4fv(modelLocation, 1, false, model, 0)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, vertexBufferId)
        GLES30.glBindBuffer(GLES30.GL_ELEMENT_ARRAY_BUFFER, indexBufferId)
        val stride = 6 * 4
        GLES30.glEnableVertexAttribArray(0)
        GLES30.glVertexAttribPointer(0, 3, GLES30.GL_FLOAT, false, stride, 0)
        GLES30.glEnableVertexAttribArray(1)
        GLES30.glVertexAttribPointer(1, 3, GLES30.GL_FLOAT, false, stride, 3 * 4)
        GLES30.glDrawElements(GLES30.GL_TRIANGLES, meshIndexCount, GLES30.GL_UNSIGNED_SHORT, 0)
        GLES30.glDisableVertexAttribArray(0)
        GLES30.glDisableVertexAttribArray(1)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, 0)
        GLES30.glBindBuffer(GLES30.GL_ELEMENT_ARRAY_BUFFER, 0)
    }

    private fun compileShader(type: Int, source: String): Int {
        val shader = GLES30.glCreateShader(type)
        GLES30.glShaderSource(shader, source)
        GLES30.glCompileShader(shader)
        val status = IntArray(1)
        GLES30.glGetShaderiv(shader, GLES30.GL_COMPILE_STATUS, status, 0)
        if (status[0] == 0) {
            val log = GLES30.glGetShaderInfoLog(shader)
            GLES30.glDeleteShader(shader)
            throw IllegalStateException("JASSIM shader compile failed: $log")
        }
        return shader
    }
}
