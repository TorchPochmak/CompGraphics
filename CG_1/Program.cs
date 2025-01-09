using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Polygon
{
    private ShaderProgram shader;
    private int vao;
    private int vbo;
    private float[] vertices;
    private int vertexCount;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public float Scale { get; set; } = 1.0f;
    public float Rotation { get; set; } = 0.0f;

    public Polygon(ShaderProgram shader, int sides)
    {
        this.shader = shader;
        InitializeVertices(sides);
        SetupGLBuffers();
    }

    private void InitializeVertices(int sides)
    {
        // Для замыкания нам нужно sides + 2: центральная точка + количество граней + 1 для замыкания
        vertexCount = sides + 2;
        vertices = new float[vertexCount * 3];

        float angleStep = 360.0f / sides;
        vertices[0] = 0.0f;  // Центр многоугольника
        vertices[1] = 0.0f;
        vertices[2] = 0.0f;

        for (int i = 0; i <= sides; i++)  // <= добавляет еще одну вершину для замыкания
        {
            float angle = MathHelper.DegreesToRadians(i * angleStep);
            vertices[(i + 1) * 3] = MathF.Cos(angle);
            vertices[(i + 1) * 3 + 1] = MathF.Sin(angle);
            vertices[(i + 1) * 3 + 2] = 0.0f;
        }
    }

    private void SetupGLBuffers()
    {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public void Draw(Matrix4 view, Matrix4 projection)
    {
        shader.Use();

        Matrix4 model = Matrix4.CreateScale(Scale) *
                        Matrix4.CreateRotationZ(Rotation) *
                        Matrix4.CreateTranslation(Position);

        int modelLocation = GL.GetUniformLocation(shader.ProgramID, "model");
        GL.UniformMatrix4(modelLocation, false, ref model);

        int viewLocation = GL.GetUniformLocation(shader.ProgramID, "view");
        GL.UniformMatrix4(viewLocation, false, ref view);

        int projectionLocation = GL.GetUniformLocation(shader.ProgramID, "projection");
        GL.UniformMatrix4(projectionLocation, false, ref projection);

        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, vertexCount + 1);
    }
}

public class MainWindow : GameWindow
{
    private ShaderProgram shader;
    private Camera camera;
    private Polygon polygon;
    private float trajectoryAngle = 0.0f;
    private float trajectoryRadius = 1.0f;
    private Vector3 manualOffset = Vector3.Zero; // Вектор смещения от ручного управления

    public MainWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings)
        : base(settings, nativeSettings)
    {
        camera = new Camera(new Vector3(0.0f, 0.0f, 5.0f));
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 position;
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            void main()
            {
                gl_Position = projection * view * model * vec4(position, 1.0);
            }
        ";

        string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(0.68, 0.85, 0.90, 1.0);
            }
        ";

        shader = new ShaderProgram(vertexShaderSource, fragmentShaderSource);
        polygon = new Polygon(shader, 5); // Создаем пятиугольник
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) return;
        var keyboardState = KeyboardState;

       

        float moveSpeed = 4.0f * (float)e.Time;
        float rotationSpeed = MathHelper.PiOver4 * (float)e.Time * 5f;
        float scaleSpeed = 2f * (float)e.Time;

        // Выход по ESC
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        // Обновление позиции из-за ручного управления
        if (keyboardState.IsKeyDown(Keys.Left))
            manualOffset.X -= moveSpeed;
        if (keyboardState.IsKeyDown(Keys.Right))
            manualOffset.X += moveSpeed;
        if (keyboardState.IsKeyDown(Keys.Up))
            manualOffset.Y += moveSpeed;
        if (keyboardState.IsKeyDown(Keys.Down))
            manualOffset.Y -= moveSpeed;

        if (keyboardState.IsKeyDown(Keys.A))
            polygon.Rotation -= rotationSpeed;
        if (keyboardState.IsKeyDown(Keys.D))
            polygon.Rotation += rotationSpeed;

        if (keyboardState.IsKeyDown(Keys.W))
            polygon.Scale += scaleSpeed;
        if (keyboardState.IsKeyDown(Keys.S))
            polygon.Scale = Math.Max(0.1f, polygon.Scale - scaleSpeed);

        // Анимация: обновление позиции многоугольника для движения по кругу
        trajectoryAngle += (float)e.Time;
        polygon.Position = new Vector3(
            MathF.Cos(trajectoryAngle) * trajectoryRadius,
            MathF.Sin(trajectoryAngle) * trajectoryRadius,
            0.0f
        ) + manualOffset; // Используем смещение, чтобы перемещать анимацию
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix(Size.X, Size.Y);

        polygon.Draw(view, projection);

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
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

        CheckProgramLinkStatus();
    }

    private int CompileShader(ShaderType shaderType, string shaderSource)
    {
        int shader = GL.CreateShader(shaderType);
        GL.ShaderSource(shader, shaderSource);
        GL.CompileShader(shader);

        string infoLog = GL.GetShaderInfoLog(shader);
        if (!string.IsNullOrEmpty(infoLog))
        {
            throw new Exception($"Error compiling {shaderType}: {infoLog}");
        }

        return shader;
    }

    private void CheckProgramLinkStatus()
    {
        GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
        {
            throw new Exception(GL.GetProgramInfoLog(ProgramID));
        }
    }

    public void Use()
    {
        GL.UseProgram(ProgramID);
    }
}
public class Camera
{
    public Vector3 Position { get; set; }
    private Vector3 front = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 up = Vector3.UnitY;

    public float Fov { get; set; } = 45.0f;
    public float AspectRatio { get; set; } = 1.0f;

    public Camera(Vector3 position)
    {
        Position = position;
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + front, up);
    }

    public Matrix4 GetProjectionMatrix(float width, float height)
    {
        AspectRatio = width / height;
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, 0.1f, 100.0f);
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
            Title = "Polygon Transformations"
        };

        using (var window = new MainWindow(gameWindowSettings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}