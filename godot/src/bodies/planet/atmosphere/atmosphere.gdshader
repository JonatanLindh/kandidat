shader_type spatial;
render_mode unshaded, depth_test_disabled;

uniform vec3 atmosphere_color : source_color = vec3(0.5, 0.7, 1.0);
uniform float thickness_scale : hint_range(0.0, 1.0, 0.01);
uniform sampler2D depth_texture : hint_depth_texture;
uniform vec3 planet_position;
uniform float planet_radius;

varying vec3 cam_pos;

vec2 raySphereIntersect(vec3 ro, vec3 rd, vec3 so, float sr, float max_depth) {
	float t = dot(so-ro, rd);
	vec3 P = ro + rd * t;
	float y = length(so-P);

	if (y > sr){
		return vec2(-1.0);
	}

	float x = sqrt(sr*sr - y*y);
	float t1 = max(t-x, 0.0);
	float t2 = min(t+x,max_depth);

	return vec2(t1,t2);
}

void vertex() {
	cam_pos = MODELVIEW_MATRIX[3].xyz;
}

void fragment() {
	vec3 cam_dir = VIEW;

	vec3 sphere_origin = planet_position;

	float sphere_radius = planet_radius * 2.;

	float depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float max_depth = length(view.xyz);

	vec2 rsi = raySphereIntersect(cam_pos, cam_dir, planet_position, planet_radius*1.5, max_depth);

	ALPHA = clamp((rsi.y-rsi.x) * thickness_scale, 0.0, 1.0);
	ALBEDO = atmosphere_color;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
