using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

class Program
{
    public static void Main()
    {
        var nativeWindowSettings = new NativeWindowSettings
        {
            Size = new Vector2i(800, 600),
            Title = "Orbit Camera with Sphere",
        };

        using (var window = new MainWindow(GameWindowSettings.Default, nativeWindowSettings))
        {
            window.Run();
        }
    }
}
public class Sphere
{
    private ShaderProgram shader;
    private int vao;
    private int vbo;
    private int ebo;
    private List<float> vertices;
    private List<uint> indices;

    public Sphere(ShaderProgram shader, int sectorCount = 36, int stackCount = 18)
    {
        this.shader = shader;

        vertices = new List<float>();
        indices = new List<uint>();

        BuildVertices(sectorCount, stackCount);

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

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }
    private void BuildVertices(int sectorCount, int stackCount)
    {
        float x, y, z, xy;                              // позиция вершин
        float sectorStep = 2 * MathF.PI / sectorCount;
        float stackStep = MathF.PI / stackCount;
        float sectorAngle, stackAngle;

        // Генерация вершин
        for (int i = 0; i <= stackCount; ++i)
        {
            stackAngle = MathF.PI / 2 - i * stackStep;  // от pi/2 до -pi/2
            xy = MathF.Cos(stackAngle);                // r * cos(u)
            z = MathF.Sin(stackAngle);                 // r * sin(u)

            // Добавление сектора
            for (int j = 0; j <= sectorCount; ++j)
            {
                sectorAngle = j * sectorStep;           // от 0 до 2pi

                x = xy * MathF.Cos(sectorAngle);        // r * cos(u) * cos(v)
                y = xy * MathF.Sin(sectorAngle);        // r * cos(u) * sin(v)

                vertices.AddRange(new float[] { x, y, z });
            }
        }

        // Генерация индексов
        for (int i = 0; i < stackCount; ++i)
        {
            uint k1 = (uint)(i * (sectorCount + 1));     // начальная вершина каждой полосы
            uint k2 = k1 + (uint)sectorCount + 1;      // начальная вершина следующей полосы

            for (int j = 0; j < sectorCount; ++j, ++k1, ++k2)
            {
                if (i != 0)
                {
                    indices.AddRange(new uint[] { k1, k2, k1 + 1 });
                }

                if (i != (stackCount - 1))
                {
                    indices.AddRange(new uint[] { k1 + 1, k2, k2 + 1 });
                }
            }
        }
    }

    public void Draw(Matrix4 view, Matrix4 projection, Matrix4 model)
    {
        shader.Use();

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
class MainWindow : GameWindow
{
    private int _vao;
    private int _vbo;
    private int _shaderProgram;

    private Vector3 _cameraPosition;
    private Vector3 _cameraTarget;
    private Vector3 _cameraUp;

    private float _yaw = -90.0f;
    private float _pitch = 0.0f;
    private float _distance = 5.0f;

    private bool _firstMouseMove = true;
    private Vector2 _lastMousePos;
    private ShaderProgram shader;
    private Sphere sphere;

    public MainWindow(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings) : base(gameSettings, nativeSettings) { }

    protected override void OnLoad()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        CreateShaders();

        _cameraPosition = new Vector3(0, 0, _distance);
        _cameraTarget = Vector3.Zero;
        _cameraUp = Vector3.UnitY;

        CursorState = CursorState.Grabbed;

        base.OnLoad();
    }

    private float[] GenerateSphereVertices(int latitudeBands, int longitudeBands)
    {
        var vertices = new System.Collections.Generic.List<float>();

        for (int lat = 0; lat <= latitudeBands; lat++)
        {
            float theta = lat * MathF.PI / latitudeBands;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= longitudeBands; lon++)
            {
                float phi = lon * 2 * MathF.PI / longitudeBands;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;

                vertices.AddRange(new[] { x, y, z });
            }
        }

        return vertices.ToArray();
    }

    private void CreateShaders()
    {
        const string vertexShaderSource = @"#version 330 core
layout(location = 0) in vec3 position;
out vec3 FragPos;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0);
    FragPos = position; 
}";

        const string fragmentShaderSource = @"#version 330 core
in vec3 FragPos;
out vec4 FragColor;
vec3 colors[8] = vec3[8](
    vec3(1.0, 0.0, 0.0), 
    vec3(0.0, 1.0, 0.0), 
    vec3(0.0, 0.0, 1.0),  
    vec3(1.0, 1.0, 0.0), 
    vec3(1.0, 0.0, 1.0),  
    vec3(0.0, 1.0, 1.0),  
    vec3(0.5, 0.5, 0.5),
    vec3(1.0, 0.5, 0.0)   
);
void main()
{
    float angle = atan(FragPos.z, FragPos.x);
    if (angle < 0.0)
        angle += 2.0 * 3.141592653589793;
    int section = int(floor((angle / (2.0 * 3.141592653589793)) * 8.0));
    FragColor = vec4(colors[section], 1.0);
}";
        shader = new ShaderProgram(vertexShaderSource, fragmentShaderSource);

        sphere = new Sphere(shader);
    }

    private void CheckShaderCompile(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation failed: {infoLog}");
        }
    }

    private void CheckProgramLink(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(program);
            throw new Exception($"Program linking failed: {infoLog}");
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (IsFocused)
        {
            var mouse = MouseState;
            if (_firstMouseMove)
            {
                _lastMousePos = new Vector2(mouse.X, mouse.Y);
                _firstMouseMove = false;
            }

            var deltaX = mouse.X - _lastMousePos.X;
            var deltaY = mouse.Y - _lastMousePos.Y;
            _lastMousePos = new Vector2(mouse.X, mouse.Y);

            const float sensitivity = 0.2f;
            _yaw += deltaX * sensitivity;
            _pitch -= deltaY * sensitivity;

            _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
            {
                _distance = MathF.Max(1.0f, _distance - 0.1f);
            }
            else if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
            {
                _distance += 0.1f;
            }

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
            }

            UpdateCameraPosition();
        }

        base.OnUpdateFrame(args);
    }

    private void UpdateCameraPosition()
    {
        float yawRad = MathHelper.DegreesToRadians(_yaw);
        float pitchRad = MathHelper.DegreesToRadians(_pitch);

        _cameraPosition.X = _distance * MathF.Cos(pitchRad) * MathF.Cos(yawRad);
        _cameraPosition.Y = _distance * MathF.Sin(pitchRad);
        _cameraPosition.Z = _distance * MathF.Cos(pitchRad) * MathF.Sin(yawRad);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(_shaderProgram);

        var model = Matrix4.Identity;
        var view = Matrix4.LookAt(_cameraPosition, _cameraTarget, _cameraUp);
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Size.X / (float)Size.Y, 0.1f, 100.0f);

        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref projection);

        sphere.Draw(view, projection, model);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, 8 * 16 * 6);

        SwapBuffers();

        base.OnRenderFrame(args);
    }

    protected override void OnUnload()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shaderProgram);

        base.OnUnload();
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