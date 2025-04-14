shader_type spatial;
render_mode unshaded;

uniform vec3 color : source_color;
uniform float star_radius;
uniform float thickness_scale : hint_range(0.0, 0.5, 0.001);
uniform float intensity : hint_range(1.0, 10.0) = 4.0;
uniform float glow_power : hint_range(1.0, 10.0) = 2.5;

varying vec3 cam_pos;
varying float corona_radius;

vec2 raySphere(vec3 sphereCenter, float sphereRadius, vec3 rayOrigin, vec3 rayDir){
	vec3 offset = rayOrigin - sphereCenter;
	float a = 1.0;
	float b = 2.0 * dot(offset, rayDir);
	float c = dot(offset, offset) - sphereRadius * sphereRadius;
	float d = b * b - 4.0 * a * c;

	if (d > 0.0) {
		float s = sqrt(d);
		float dstToSphereNear = max(0.0, (-b - s) / (2.0 * a));
		float dstToSphereFar = (-b + s) / (2.0 * a);

		if (dstToSphereFar > 0.0) {
			return vec2(dstToSphereNear, dstToSphereFar);
		}
	}
	return vec2(100000.0, 0.0);
}

void vertex() {
	cam_pos = MODELVIEW_MATRIX[3].xyz;
}

void fragment() {
	vec3 cam_dir = normalize(VIEW); // Just to be safe
	corona_radius = star_radius * 1.2;

	vec2 rsi = raySphere(NODE_POSITION_WORLD, corona_radius, cam_pos, cam_dir);
	float ray_length = rsi.y - rsi.x;

	if (ray_length <= 0.0) {
		discard; // We're not hitting the corona at all
	}

	// Find sample point in the middle of the segment inside corona
	vec3 sample_point = cam_pos + cam_dir * (rsi.x + ray_length * 0.5);
	float dist_to_center = distance(sample_point, NODE_POSITION_WORLD);

	// Get normalized falloff from center of star to edge of corona
	float glow_factor = 1.0 - smoothstep(star_radius, corona_radius, dist_to_center);

	// Optionally apply power curve for glow shaping
	glow_factor = pow(glow_factor, glow_power);

	vec3 glow_color = color * glow_factor * intensity;

	ALBEDO = glow_color;
	ALPHA = glow_factor * thickness_scale;
}
