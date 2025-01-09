using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

public class CylinderApp : GameWindow
{
    private ShaderProgram shader;
    private LateralCamera camera;
    private Cylinder cylinder;

    public CylinderApp(GameWindowSettings settings, NativeWindowSettings nativeSettings)
        : base(settings, nativeSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        string vertexShaderSource = @"
#version 330 core
layout(location = 0) in vec3 position;
out vec3 FragPos;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main()
{
    FragPos = vec3(model * vec4(position, 1.0));
    gl_Position = projection * view * vec4(FragPos, 1.0);
}
";

        string fragmentShaderSource = @"
#version 330 core
in vec3 FragPos;
out vec4 FragColor;
void main()
{
    float offsetX = 1;
    float distanceFromCenter = length(vec2(FragPos.x + offsetX, FragPos.z));
    float gradient = distanceFromCenter; 
    gradient = fract(gradient);
    vec3 baseColor = vec3(1, 1, 1);
    vec3 edgeColor = vec3(0.1, 0.1, 0.1); 
    FragColor = vec4(mix(baseColor, edgeColor, gradient), 1.0);
}
";

        shader = new ShaderProgram(vertexShaderSource, fragmentShaderSource);
        camera = new LateralCamera(3.0f, 1.5f);
        cylinder = new Cylinder(shader);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        var keyboardState = KeyboardState;
        camera.Update((float)e.Time, keyboardState);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix(Size.X, Size.Y);

        cylinder.Draw(view, projection, shader);

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}

class Program
{
    static void Main(string[] args)
    {
        var gameWindowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "3D Cylinder"
        };

        using (var window = new CylinderApp(gameWindowSettings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}

public class LateralCamera
{
    public Vector3 Position { get; private set; }
    private Vector3 lookAt = Vector3.Zero;
    private float radius;
    private float angle;

    public float Height { get; set; }

    public LateralCamera(float initialRadius, float initialHeight)
    {
        radius = initialRadius;
        Height = initialHeight;
        angle = 0.0f; 
        UpdatePosition();
    }

    public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, lookAt, Vector3.UnitY);

    public Matrix4 GetProjectionMatrix(float width, float height)
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / height, 0.1f, 100.0f);
    }

    public void Update(float deltaTime, KeyboardState keyboardState)
    {
        float angleSpeed = MathHelper.DegreesToRadians(30.0f); 
        const float radiusSpeed = 1.0f; 


        if (keyboardState.IsKeyDown(Keys.W))
            Height += radiusSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            Height -= radiusSpeed * deltaTime;


        if (keyboardState.IsKeyDown(Keys.A))
            angle -= angleSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            angle += angleSpeed * deltaTime;


        if (keyboardState.IsKeyDown(Keys.Up))
            radius = MathF.Max(radius - radiusSpeed * deltaTime, 0.1f); 
        if (keyboardState.IsKeyDown(Keys.Down))
            radius += radiusSpeed * deltaTime;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Position = new Vector3(
            MathF.Cos(angle) * radius,
            Height,
            MathF.Sin(angle) * radius
        );
    }
}

public class ShaderProgram
{
    public int ProgramID { get; private set; }

    public ShaderProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

        ProgramID = GL.CreateProgram();
        GL.AttachShader(ProgramID, vertexShader);
        GL.AttachShader(ProgramID, fragmentShader);
        GL.LinkProgram(ProgramID);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
            throw new System.Exception(GL.GetProgramInfoLog(ProgramID));
    }

    private int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
            throw new System.Exception(GL.GetShaderInfoLog(shader));
        return shader;
    }

    public void Use()
    {
        GL.UseProgram(ProgramID);
    }
}

public class Cylinder
{
    private ShaderProgram shader;
    private int vao;
    private int vbo;
    private int ebo;
    private List<float> vertices;
    private List<uint> indices;

    public Cylinder(ShaderProgram shader, int segments = 36, float height = 1.0f, float radius = 0.5f)
    {
        this.shader = shader;
        vertices = new List<float>();
        indices = new List<uint>();
        GenerateCylinder(segments, height, radius);
        SetupGLBuffers();
    }

    private void GenerateCylinder(int segments, float height, float radius)
    {
        float angleStep = 2 * MathF.PI / segments;
        int vertexCount = 0;


        for (int i = 0; i < 2; i++)
        {
            float y = (i == 0) ? -height / 2 : height / 2;


            vertices.AddRange(new float[] { 0f, y, 0f });

            for (int j = 0; j < segments; j++)
            {
                float angle = j * angleStep;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;
                vertices.AddRange(new float[] { x, y, z });

                if (j < segments - 1)
                {
                    indices.Add((uint)(vertexCount));
                    indices.Add((uint)(vertexCount + j + 1));
                    indices.Add((uint)(vertexCount + j + 2));
                }
                else 
                {
                    indices.Add((uint)(vertexCount));
                    indices.Add((uint)(vertexCount + j + 1));
                    indices.Add((uint)(vertexCount + 1));
                }
            }

            vertexCount += segments + 1;
        }

        for (int j = 0; j < segments; j++)
        {
            float angle = j * angleStep;
            float x = MathF.Cos(angle) * radius;
            float z = MathF.Sin(angle) * radius;


            vertices.AddRange(new float[] { x, -height / 2, z });

            vertices.AddRange(new float[] { x, height / 2, z });

            int lowerVertex = vertexCount + 2 * j;
            int upperVertex = lowerVertex + 1;

            if (j < segments - 1)
            {
                int nextLowerVertex = lowerVertex + 2;
                int nextUpperVertex = upperVertex + 2;

                indices.AddRange(new uint[] { (uint)lowerVertex, (uint)nextLowerVertex, (uint)upperVertex });
                indices.AddRange(new uint[] { (uint)upperVertex, (uint)nextLowerVertex, (uint)nextUpperVertex });
            }
            else 
            {
                indices.AddRange(new uint[] { (uint)lowerVertex, (uint)vertexCount, (uint)upperVertex });
                indices.AddRange(new uint[] { (uint)upperVertex, (uint)vertexCount, (uint)(vertexCount + 1) });
            }
        }
    }

    private void SetupGLBuffers()
    {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    public void Draw(Matrix4 view, Matrix4 projection, ShaderProgram shader)
    {
        shader.Use();

        Matrix4 model = Matrix4.Identity; 
        int modelLocation = GL.GetUniformLocation(shader.ProgramID, "model");
        GL.UniformMatrix4(modelLocation, false, ref model);

        int viewLocation = GL.GetUniformLocation(shader.ProgramID, "view");
        GL.UniformMatrix4(viewLocation, false, ref view);

        int projectionLocation = GL.GetUniformLocation(shader.ProgramID, "projection");
        GL.UniformMatrix4(projectionLocation, false, ref projection);

        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
    }
}