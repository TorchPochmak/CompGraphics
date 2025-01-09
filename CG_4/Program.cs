using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;

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

public class Pyramid
{
    private ShaderProgram shader;
    private int vao;
    private int vbo;

    // Измените количество элементов на размеры одного набора данных 
    private readonly static float[] PyramidVertices = {
        // Positions          // Normals            // Texture Coords
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,   1.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,   1.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,   0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,   0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,   0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,   1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f,  0.5f,  0.0f,   0.0f, 0.0f,
         0.0f,  0.5f,  0.0f,  0.0f,  0.5f,  0.0f,   0.5f, 0.5f,
         0.5f, -0.5f, -0.5f,  0.0f,  0.5f,  0.0f,   1.0f, 0.0f,

         0.5f, -0.5f, -0.5f,  0.0f,  0.5f,  0.0f,   1.0f, 0.0f,
         0.0f,  0.5f,  0.0f,  0.0f,  0.5f,  0.0f,   0.5f, 0.5f,
         0.5f, -0.5f,  0.5f,  0.0f,  0.5f,  0.0f,   1.0f, 1.0f,

         0.5f, -0.5f,  0.5f,  0.0f,  0.5f,  0.0f,   1.0f, 1.0f,
         0.0f,  0.5f,  0.0f,  0.0f,  0.5f,  0.0f,   0.5f, 0.5f,
        -0.5f, -0.5f,  0.5f,  0.0f,  0.5f,  0.0f,   0.0f, 1.0f,

        -0.5f, -0.5f,  0.5f,  0.0f,  0.5f,  0.0f,   0.0f, 1.0f,
         0.0f,  0.5f,  0.0f,  0.0f,  0.5f,  0.0f,   0.5f, 0.5f,
        -0.5f, -0.5f, -0.5f,  0.0f,  0.5f,  0.0f,   0.0f, 0.0f,
    };

    public Pyramid(ShaderProgram shader)
    {
        this.shader = shader;
        SetupGeometry();
    }

    private void SetupGeometry()
    {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, PyramidVertices.Length * sizeof(float), PyramidVertices, BufferUsageHint.StaticDraw);

        // Позиция атрибута вершины
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Нормали
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Текстурные координаты
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(0);
    }

    public void Draw(Matrix4 model, Matrix4 view, Matrix4 projection)
    {
        shader.Use();

        GL.UniformMatrix4(GL.GetUniformLocation(shader.ProgramID, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(shader.ProgramID, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shader.ProgramID, "projection"), false, ref projection);

        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, PyramidVertices.Length / 8);
        GL.BindVertexArray(0);
    }
}
public class Spotlight
{
    public Vector3 Position { get; set; } = new Vector3(0.0f, 3.0f, 2.0f);
    public Vector3 Direction { get; set; } = new Vector3(-1.0f, 0.0f, 0.0f);
    public Vector3 LightColor { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
    public float CutOff { get; set; }
    public float OuterCutOff { get; set; }

    // Добавим конструктор, если необходимо
    public Spotlight(float cutOff, float outerCutOff)
    {
        CutOff = (float)MathHelper.Cos(MathHelper.DegreesToRadians(cutOff));
        OuterCutOff = (float)MathHelper.Cos(MathHelper.DegreesToRadians(outerCutOff));
    }
}
//public class MainApp : GameWindow
//{
//    private ShaderProgram shader;
//    private Pyramid pyramid;
//    private Spotlight spotlight;
//    private OrbitCamera camera;

//    public MainApp(GameWindowSettings settings, NativeWindowSettings nativeSettings)
//        : base(settings, nativeSettings)
//    {
//    }

//    protected override void OnLoad()
//    {
//        base.OnLoad();
//        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
//        GL.Enable(EnableCap.DepthTest);

//        string vertexShaderSource = @"
//            #version 330 core
//            layout(location = 0) in vec3 position;

//            uniform mat4 model;
//            uniform mat4 view;
//            uniform mat4 projection;
//            uniform vec3 lightPos;
//            uniform vec3 viewPos;
//            uniform vec3 lightDir;
//            uniform float cutOff;
//            uniform float outerCutOff;

//            void main()
//            {
//                gl_Position = projection * view * model * vec4(position, 1.0);
//            }
//        ";

//        string fragmentShaderSource = @"
//            #version 330 core
//            out vec4 FragColor;

//            uniform vec3 lightPos;
//            uniform vec3 viewPos;
//            uniform vec3 lightDir;
//            uniform float cutOff;
//            uniform float outerCutOff;

//            void main()
//            {
//                vec3 lightColor = vec3(1.0f);
//                FragColor = vec4(lightColor, 1.0f); // Replace this with your spotlight calculation
//            }
//        ";

//        shader = new ShaderProgram(vertexShaderSource, fragmentShaderSource);
//        pyramid = new Pyramid(shader);
//        spotlight = new Spotlight(12.5f, 17.5f);
//        camera = new OrbitCamera(5.0f, Vector3.Zero);
//    }

//    protected override void OnUpdateFrame(FrameEventArgs e)
//    {
//        base.OnUpdateFrame(e);

//        // Обновление позиции и направления прожектора
//        // Возможность изменения направления и угла прожектора
//    }

//    protected override void OnRenderFrame(FrameEventArgs e)
//    {
//        base.OnRenderFrame(e);
//        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

//        Matrix4 view = camera.GetViewMatrix();
//        Matrix4 projection = camera.GetProjectionMatrix(Size.X, Size.Y);
//        // Установим позиции света, камеры, направления и значений cut-off
//        shader.Use();
//        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "lightPos"), spotlight.Position);
//        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "viewPos"), camera.Position);
//        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "lightDir"), spotlight.Direction);
//        GL.Uniform1(GL.GetUniformLocation(shader.ProgramID, "cutOff"), spotlight.CutOff);
//        GL.Uniform1(GL.GetUniformLocation(shader.ProgramID, "outerCutOff"), spotlight.OuterCutOff);

//        // Отрисовка пирамиды
//        pyramid.Draw(view, projection);

//        Context.SwapBuffers();
//    }

//    protected override void OnResize(ResizeEventArgs e)
//    {
//        base.OnResize(e);
//        GL.Viewport(0, 0, Size.X, Size.Y);
//    }
//}
public class MainApp : GameWindow
{
    private Stopwatch _timer = new Stopwatch();
    private ShaderProgram shader;
    private Pyramid pyramid;
    private Pyramid pyr2;
    private Spotlight spotlight;
    private OrbitCamera camera;

    public MainApp(GameWindowSettings settings, NativeWindowSettings nativeSettings)
        : base(settings, nativeSettings)
    {
    }

    protected override void OnLoad()
    {
        _timer.Start(); // Запускаем таймер при загрузке
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        string vertexShaderSource = @"
#version 330 core
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;

out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    FragPos = vec3(model * vec4(position, 1.0));
    Normal = mat3(transpose(inverse(model))) * normal;
    gl_Position = projection * view * vec4(FragPos, 1.0);
}
        ";

        string fragmentShaderSource = @"
#version 330 core
in vec3 FragPos;
in vec3 Normal;
out vec4 FragColor;
uniform vec3 lightPos;
uniform vec3 lightDir;
uniform float cutOff;
uniform float outerCutOff;
uniform vec3 lightColor;
uniform vec3 objectColor;
void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDirNorm = normalize(lightDir);
    vec3 fragToLight = normalize(lightPos - FragPos);
    float theta = dot(fragToLight, lightDirNorm);
    float epsilon = cutOff - outerCutOff;
    float intensity = clamp((theta - outerCutOff) / epsilon, 0.0, 1.0);
    float diffuseFactor = max(dot(norm, fragToLight), 0.0);
    vec3 diffuse = diffuseFactor * lightColor * intensity;
    vec3 lighting = diffuse;
    FragColor = vec4(lighting * objectColor, 1.0);
}
        ";

        shader = new ShaderProgram(vertexShaderSource, fragmentShaderSource);
        pyramid = new Pyramid(shader);
        pyr2 = new Pyramid(shader);
        spotlight = new Spotlight(MathHelper.DegreesToRadians(12.5f), MathHelper.DegreesToRadians(17.5f));
        camera = new OrbitCamera(5.0f, Vector3.Zero);
        spotlight.CutOff = 0.01f;
        spotlight.OuterCutOff = 0.01f;
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        float cameraSpeed = 5f * (float)e.Time;
        float rotationSpeed = 1000f * (float)e.Time;

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        // Управление позицией прожектора
        // Обработка вращения камеры вокруг цели
        if (KeyboardState.IsKeyDown(Keys.A))
        {
            camera.ProcessMouseMovement(rotationSpeed, 0);
        }
        if (KeyboardState.IsKeyDown(Keys.D))
        {
            camera.ProcessMouseMovement(-rotationSpeed, 0);
        }
        if (KeyboardState.IsKeyDown(Keys.W))
        {
            camera.ProcessMouseMovement(0, rotationSpeed);
        }
        if (KeyboardState.IsKeyDown(Keys.S))
        {
            camera.ProcessMouseMovement(0, -rotationSpeed);
        }
        float speed = 0.1f;
        // Изменение углов прожектора
        // Уменьшение/увеличение углов плавно с шагом 0.01 радиана в секунду
        float cutoffAdjustment = 0.01f * (float)e.Time; // Угол изменяется на 0.01 радиан в секунду

        if (KeyboardState.IsKeyDown(Keys.Z))
        {
            spotlight.CutOff = MathHelper.Clamp(spotlight.CutOff - MathHelper.DegreesToRadians(0.01f), MathHelper.DegreesToRadians(1.0f), spotlight.OuterCutOff - MathHelper.DegreesToRadians(1.0f));
        }
        if (KeyboardState.IsKeyDown(Keys.X))
        {
            spotlight.CutOff = MathHelper.Clamp(spotlight.CutOff + MathHelper.DegreesToRadians(0.01f), MathHelper.DegreesToRadians(1.0f), spotlight.OuterCutOff - MathHelper.DegreesToRadians(1.0f));
        }
        if (KeyboardState.IsKeyDown(Keys.C))
        {
            spotlight.OuterCutOff = MathHelper.Clamp(spotlight.OuterCutOff - MathHelper.DegreesToRadians(0.01f), spotlight.CutOff + MathHelper.DegreesToRadians(1.0f), MathHelper.DegreesToRadians(60.0f));
        }
        if (KeyboardState.IsKeyDown(Keys.V))
        {
            spotlight.OuterCutOff = MathHelper.Clamp(spotlight.OuterCutOff + MathHelper.DegreesToRadians(0.01f), spotlight.CutOff + MathHelper.DegreesToRadians(1.0f), MathHelper.DegreesToRadians(60.0f));
        }
        // Вращение прожектора
        if (KeyboardState.IsKeyDown(Keys.Left))
        {
            spotlight.Direction = Vector3.Transform(spotlight.Direction, Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(0.01f)));
        }
        if (KeyboardState.IsKeyDown(Keys.Right))
        {
            spotlight.Direction = Vector3.Transform(spotlight.Direction, Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-0.01f)));
        }
        if (KeyboardState.IsKeyDown(Keys.Up))
        {
            spotlight.Direction = Vector3.Transform(spotlight.Direction, Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(0.01f)));
        }
        if (KeyboardState.IsKeyDown(Keys.Down))
        {
            spotlight.Direction = Vector3.Transform(spotlight.Direction, Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(-0.01f)));
        }

        // Каждые 5 секунд выводим позиции камеры и света
        if (_timer.ElapsedMilliseconds >= 1000)
        {
            Console.WriteLine("Cam: " + camera.Position);
            Console.WriteLine("Light: " + spotlight.Direction);
            Console.WriteLine($"CutOff: {spotlight.CutOff} OuterCutOf:{spotlight.OuterCutOff}");
            _timer.Restart();
        }
    }
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix(Size.X, Size.Y);

        shader.Use();

        // Обновите позиции и другие параметры освещения
        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "lightPos"), spotlight.Position);
        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "viewPos"), camera.Position);
        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "lightDir"), spotlight.Direction);
        GL.Uniform1(GL.GetUniformLocation(shader.ProgramID, "cutOff"), spotlight.CutOff);
        GL.Uniform1(GL.GetUniformLocation(shader.ProgramID, "outerCutOff"), spotlight.OuterCutOff);
        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "lightColor"), new Vector3(1.0f, 1.0f, 1.0f)); // Белый свет
        GL.Uniform3(GL.GetUniformLocation(shader.ProgramID, "objectColor"), new Vector3(1.0f, 0.5f, 0.31f)); // Цвет объекта

        pyramid.Draw(Matrix4.Identity, view, projection);

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}
public class OrbitCamera
{
    private Vector3 target;
    private float distance;
    private float yaw, pitch;

    public Vector3 Position { get; private set; }

    public OrbitCamera(float initialDistance, Vector3 target)
    {
        this.target = target;
        distance = initialDistance;
        yaw = -90.0f; // начальный угол поворота
        pitch = 0.0f;
        UpdatePosition();
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, target, Vector3.UnitY);
    }

    public Matrix4 GetProjectionMatrix(float width, float height)
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / height, 0.1f, 100.0f);
    }

    public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
    {
        const float sensitivity = 0.1f;
        xOffset *= sensitivity;
        yOffset *= sensitivity;

        yaw += xOffset;
        pitch -= yOffset;

        if (constrainPitch)
        {
            pitch = MathHelper.Clamp(pitch, -89.0f, 89.0f);
        }

        UpdatePosition();
    }

    public void ProcessMouseScroll(float yOffset)
    {
        distance = MathHelper.Clamp(distance - yOffset, 1.0f, 10.0f);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float radPitch = MathHelper.DegreesToRadians(pitch);
        float radYaw = MathHelper.DegreesToRadians(yaw);

        Position = new Vector3(target.X + distance * MathF.Cos(radPitch) * MathF.Cos(radYaw), Position.Y, Position.Z);
        Position = new Vector3(Position.X, target.Y + distance * MathF.Sin(radPitch), Position.Z);
        Position = new Vector3(Position.X, Position.Y, target.Z + distance * MathF.Cos(radPitch) * MathF.Sin(radYaw));
    }
}
class Program
{
    public static void Main(string[] args)
    {
        var gameWindowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "3D Pyramid with Spotlight"
        };

        using (var window = new MainApp(gameWindowSettings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}