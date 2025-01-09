#include <SFML/Graphics.hpp>
#include <iostream>
#include <vector>
#include <cmath>
#include <cstdlib>

class Vec3 {
public:
    float x, y, z;

    Vec3() : x(0), y(0), z(0) {}
    Vec3(float x, float y, float z) : x(x), y(y), z(z) {}

    Vec3 operator+(const Vec3& other) const {
        return Vec3(x + other.x, y + other.y, z + other.z);
    }
    Vec3 operator-(const Vec3& other) const {
        return Vec3(x - other.x, y - other.y, z - other.z);
    }
    Vec3 operator*(float scalar) const {
        return Vec3(x * scalar, y * scalar, z * scalar);
    }
    Vec3 operator/(float scalar) const {
        return Vec3(x / scalar, y / scalar, z / scalar);
    }

    float dot(const Vec3& other) const {
        return x * other.x + y * other.y + z * other.z;
    }
    Vec3 cross(const Vec3& other) const {
        return Vec3(
            y * other.z - z * other.y,
            z * other.x - x * other.z,
            x * other.y - y * other.x
        );
    }

    float length() const {
        return std::sqrt(x * x + y * y + z * z);
    }
    Vec3 normalize() const {
        return *this / length();
    }
};

struct Ray {
    Vec3 origin;
    Vec3 direction;

    Ray(const Vec3& origin, const Vec3& direction) : origin(origin), direction(direction) {}
};

struct HitRecord {
    Vec3 point;
    Vec3 normal;
    float t;
    bool hit;
};

class Sphere {
public:
    Vec3 center;
    float radius;

    Sphere(const Vec3& center, float radius) : center(center), radius(radius) {}

    bool intersect(const Ray& ray, HitRecord& hitRecord) const {
        Vec3 oc = ray.origin - center;
        float a = ray.direction.dot(ray.direction);
        float b = 2.0f * oc.dot(ray.direction);
        float c = oc.dot(oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant > 0) {
            float sqrtDiscriminant = std::sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);
            if (t1 > 0) {
                hitRecord.t = t1;
                hitRecord.point = ray.origin + ray.direction * t1;
                hitRecord.normal = (hitRecord.point - center).normalize();
                hitRecord.hit = true;
                return true;
            }
            if (t2 > 0) {
                hitRecord.t = t2;
                hitRecord.point = ray.origin + ray.direction * t2;
                hitRecord.normal = (hitRecord.point - center).normalize();
                hitRecord.hit = true;
                return true;
            }
        }
        hitRecord.hit = false;
        return false;
    }
};

class Sphere2 {
public:
    Vec3 center;
    float radius;

    Sphere2(const Vec3& center, float radius) : center(center), radius(radius) {}

    bool intersect(const Ray& ray, HitRecord& hitRecord) const {
        Vec3 oc = ray.origin - center;
        float a = ray.direction.dot(ray.direction);
        float b = 2.0f * oc.dot(ray.direction);
        float c = oc.dot(oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant > 0) {
            float sqrtDiscriminant = std::sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);
            if (t1 > 0) {
                hitRecord.t = t1;
                hitRecord.point = ray.origin + ray.direction * t1;
                hitRecord.normal = (hitRecord.point - center).normalize();
                hitRecord.hit = true;
                return true;
            }
            if (t2 > 0) {
                hitRecord.t = t2;
                hitRecord.point = ray.origin + ray.direction * t2;
                hitRecord.normal = (hitRecord.point - center).normalize();
                hitRecord.hit = true;
                return true;
            }
        }
        hitRecord.hit = false;
        return false;
    }
};
class Sphere3 {
public:
    Vec3 center;
    float radius;

    Sphere3(const Vec3& center, float radius) : center(center), radius(radius) {}

    bool intersect(const Ray& ray, HitRecord& hitRecord) const {
        Vec3 oc = ray.origin - center;
        float a = ray.direction.dot(ray.direction);
        float b = 2.0f * oc.dot(ray.direction);
        float c = oc.dot(oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant > 0) {
            float sqrtDiscriminant = std::sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);
            if (t1 > 0) {
                hitRecord.t = t1;
                hitRecord.point = ray.origin + ray.direction * t1;
                hitRecord.normal = (hitRecord.point - center).normalize();
                hitRecord.hit = true;
                return true;
            }
            if (t2 > 0) {
                hitRecord.t = t2;
                hitRecord.point = ray.origin + ray.direction * t2;
                hitRecord.normal = (hitRecord.point - center).normalize();
                hitRecord.hit = true;
                return true;
            }
        }
        hitRecord.hit = false;
        return false;
    }
};

class Plane {
public:
    Vec3 point;
    Vec3 normal;

    Plane(const Vec3& point, const Vec3& normal) : point(point), normal(normal) {}

    bool intersect(const Ray& ray, HitRecord& hitRecord) const {
        float denom = normal.dot(ray.direction);
        if (std::abs(denom) > 1e-6) {
            float t = (point - ray.origin).dot(normal) / denom;
            if (t > 0) {
                hitRecord.t = t;
                hitRecord.point = ray.origin + ray.direction * t;
                hitRecord.normal = normal;
                hitRecord.hit = true;
                return true;
            }
        }
        hitRecord.hit = false;
        return false;
    }
};

Vec3 randomInHemisphere(const Vec3& normal) {
    float theta = static_cast<float>(rand()) / RAND_MAX * M_PI;
    float phi = static_cast<float>(rand()) / RAND_MAX * 2 * M_PI;
    float x = std::sin(theta) * std::cos(phi);
    float y = std::sin(theta) * std::sin(phi);
    float z = std::cos(theta);
    Vec3 randomDir = Vec3(x, y, z);
    return (randomDir.dot(normal) > 0)
           ? randomDir
           : randomDir * -1.0f;
}

Vec3 trace(
    const Ray& ray,
    const std::vector<Sphere>& spheres,
    const std::vector<Sphere2>& spheres2,
    const std::vector<Sphere3>& spheres3,
    const Plane& plane,
    int depth,
    int maxDepth,
    int shadowSamples
)
{
    if (depth > maxDepth) return Vec3(0, 0, 0);

    HitRecord hitRecord;
    bool hit = false;
    float closestT = std::numeric_limits<float>::max();


    for (const auto& sphere : spheres) {
        HitRecord tempHit;
        if (sphere.intersect(ray, tempHit) && tempHit.t < closestT) {
            closestT = tempHit.t;
            hitRecord = tempHit;
            hit = true;
        }
    }


    for (const auto& sphere2 : spheres2) {
        HitRecord tempHit;
        if (sphere2.intersect(ray, tempHit) && tempHit.t < closestT) {
            return Vec3(0, 0, 0);  
        }
    }


    for (const auto& sphere3 : spheres3) {
        HitRecord tempHit;
        if (sphere3.intersect(ray, tempHit) && tempHit.t < closestT) {
            return Vec3(1, 1, 1);  
        }
    }

    HitRecord planeHit;
    if (plane.intersect(ray, planeHit) && planeHit.t < closestT) {
        closestT = planeHit.t;
        hitRecord = planeHit;
        hit = true;
    }

    if (!hit) return Vec3(0.2, 0.2, 0.2);

    Vec3 color(0.1, 0.1, 0.1);
    Vec3 lightPos(5, 5, -5);
    Vec3 lightColor(1.0, 1.0, 1.0);
    Vec3 toLight = (lightPos - hitRecord.point).normalize();

    bool inShadow = false;
    for (int i = 0; i < shadowSamples; ++i) {
        Vec3 randomLightPos = lightPos + Vec3(
            (static_cast<float>(rand()) / RAND_MAX - 0.5f) * 0.5f,
            (static_cast<float>(rand()) / RAND_MAX - 0.5f) * 0.5f,
            (static_cast<float>(rand()) / RAND_MAX - 0.5f) * 0.5f
        );

        Vec3 jitteredDir = (randomLightPos - hitRecord.point).normalize();
        Ray shadowRay(hitRecord.point + hitRecord.normal * 1e-3, jitteredDir);

        for (const auto& sphere : spheres) {
            HitRecord shadowHit;
            if (sphere.intersect(shadowRay, shadowHit)) {
                inShadow = true;
                break;
            }
        }
        if (plane.intersect(shadowRay, planeHit)) {
            inShadow = true;
        }

        if (inShadow) break;
    }

    float shadowFactor = inShadow ? 0.2f : 1.0f;

    float diffuse = std::max(0.0f, hitRecord.normal.dot(toLight));
    color = color + lightColor * diffuse * shadowFactor;

    if (depth < maxDepth) {
        Vec3 reflectedDir = ray.direction - hitRecord.normal * 2.0f * ray.direction.dot(hitRecord.normal);
        Ray reflectedRay(hitRecord.point + hitRecord.normal * 1e-3, reflectedDir);
        Vec3 reflectedColor = trace(reflectedRay, spheres, spheres2, spheres3, plane, depth + 1, maxDepth, shadowSamples);

        float fresnel = 0.04 + 0.96 * pow(1.0 - std::max(0.0f, ray.direction.dot(hitRecord.normal)), 5);
        color = color + reflectedColor * fresnel;
    }
    float ao = 0.0f;
    for (int i = 0; i < shadowSamples; ++i) {
        Vec3 randomDir = hitRecord.normal + Vec3(
            (static_cast<float>(rand()) / RAND_MAX - 0.5f) * 2.0f,
            (static_cast<float>(rand()) / RAND_MAX - 0.5f) * 2.0f,
            (static_cast<float>(rand()) / RAND_MAX - 0.5f) * 2.0f
        ).normalize();
        Ray aoRay(hitRecord.point + hitRecord.normal * 1e-3, randomDir);

        bool aoHit = false;
        for (const auto& sphere : spheres) {
            HitRecord aoHitRecord;
            if (sphere.intersect(aoRay, aoHitRecord)) {
                aoHit = true;
                break;
            }
        }
        if (plane.intersect(aoRay, planeHit)) aoHit = true;

        if (!aoHit) ao += 1.0f;
    }
    ao /= shadowSamples;
    color = color * (0.5f + 0.5f * ao);

    color = Vec3(
        pow(color.x, 1.0f / 2.2f),
        pow(color.y, 1.0f / 2.2f),
        pow(color.z, 1.0f / 2.2f)
    );

    return color;
}

int main() {
    srand(static_cast<unsigned>(time(0)));

    const int width = 800;
    const int height = 600;
    int samples = 100;

    std::vector<Sphere> spheres = {
        Sphere(Vec3(-2, 1, -5), 1)
    };
    std::vector<Sphere2> spheres2 = {
        };
    for (int i = -5; i <=5; i++) {
        for (int j = -5; j <= 5; j++) {
            spheres2.push_back(Sphere2(Vec3(i, j, -6), 0.3));
        }
    }
    std::vector<Sphere3> spheres3 = {
        Sphere3(Vec3(0, 1, -5), 1)
    };

    Plane floor(Vec3(0, -100, 0), Vec3(0, 1, 0));


    sf::RenderWindow window(sf::VideoMode(width, height), "Ray Tracing Scene");

    sf::Texture texture;
    texture.create(width, height);
    sf::Sprite sprite(texture);

    std::vector<sf::Uint8> pixels(width * height * 4);  


    for (int y = 0; y < height; ++y) {
      for (int x = 0; x < width; ++x) {
          float u = float(x) / float(width);

          float v = float(height - y - 1) / float(height);

          Vec3 origin(0, 0, 0);
          Vec3 direction(u * 2 - 1, v * 2 - 1, -1);
          Ray ray(origin, direction.normalize());

          Vec3 color = trace(ray, spheres, spheres2, spheres3, floor, 0, 5, 20);

          int r = std::min(255, int(color.x * 255));
          int g = std::min(255, int(color.y * 255));
          int b = std::min(255, int(color.z * 255));

          pixels[(y * width + x) * 4] = r;
          pixels[(y * width + x) * 4 + 1] = g;
          pixels[(y * width + x) * 4 + 2] = b;
          pixels[(y * width + x) * 4 + 3] = 255;  
      }
    }


    texture.update(pixels.data());

    // Main render loop
    while (window.isOpen()) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed)
                window.close();
        }

        window.clear();
        window.draw(sprite);
        window.display();
    }

    return 0;
}