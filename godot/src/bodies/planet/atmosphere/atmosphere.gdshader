shader_type spatial;
render_mode unshaded, depth_test_disabled;

uniform vec3 atmosphere_color : source_color = vec3(0.5, 0.7, 1.0);
uniform vec3 planet_position;
uniform vec3 sun_direction = vec3(0.,0.,-1.);
uniform float thickness_scale : hint_range(0.0, 0.5, 0.001);
uniform sampler2D depth_texture : hint_depth_texture;
uniform float planet_radius;
uniform float density_falloff : hint_range(-10.0, 10.0, 0.1);
uniform int num_in_scattering_points : hint_range(0, 100, 1);
uniform int num_optical_depth_points : hint_range(0, 100, 1);

varying vec3 cam_pos;
varying float atmosphere_radius;

vec2 ray_sphere_intersect(vec3 ro, vec3 rd, vec3 so, float sr, float max_depth) {
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

vec2 raySphere2(vec3 sphereCenter, float sphereRadius, vec3 rayOrigin, vec3 rayDir){
	vec3 offset = rayOrigin - sphereCenter;
	float a = dot(rayDir, rayDir);
	float b = 2. * dot(offset, rayDir);
	float c = dot(offset,offset) - sphereRadius * sphereRadius;
	float d = b * b - 4. * a * c;

	if(d > 0.){
		float s = sqrt(d);
		float dstToSphereNear = max(0, (-b-s) / (2. * a));
		float dstToSphereFar = (-b + s) / (2. * a);

		if (dstToSphereFar >= 0.){
			return vec2(dstToSphereNear, dstToSphereFar-dstToSphereNear);
		}
	}
	return vec2(100000., 0);
}


float density_at_point(vec3 density_sample_point){
	float height_above_surface = length(density_sample_point - planet_position) - planet_radius;
	float height01 = height_above_surface / (atmosphere_radius - planet_radius);
	float local_density = exp(-height01 * density_falloff) * (1. - height01);
	return local_density;
}

float optical_depth(vec3 ray_origin, vec3 ray_dir, float ray_length){
	vec3 density_sample_point = ray_origin;
	float step_size = ray_length / (float(num_optical_depth_points) - 1.);
	float optical_depth = 0.;

	for(int i = 0; i < num_optical_depth_points; i++) {
		float local_density = density_at_point(density_sample_point);
		optical_depth += local_density * step_size;
		density_sample_point += ray_dir * step_size;
	}

	return optical_depth;
}

float calculate_light(vec3 ray_origin, vec3 ray_dir, float ray_length){
	vec3 in_scatter_point = ray_origin;
	float step_size = ray_length / (float(num_in_scattering_points)-1.);
	float in_scattered_light = 0.;
	for(int i = 0; i < num_in_scattering_points; i++){
		//float sun_ray_length = ray_sphere_intersect(in_scatter_point, sun_direction, planet_position, atmosphere_radius, 10000.).y;
		float sun_ray_length = raySphere2(planet_position, atmosphere_radius, in_scatter_point, -sun_direction).y;
		float sun_ray_optical_depth = optical_depth(in_scatter_point, sun_direction, sun_ray_length);
		float view_ray_optical_depth = optical_depth(in_scatter_point, -ray_dir, step_size*float(i));
		float transmittance = exp(-(sun_ray_optical_depth + view_ray_optical_depth));
		float local_density = density_at_point(in_scatter_point);

		in_scattered_light += local_density * transmittance * step_size;
		in_scatter_point += ray_dir * step_size;
	}
	return in_scattered_light;
}

void vertex() {
	cam_pos = MODELVIEW_MATRIX[3].xyz;

}

void fragment() {
	atmosphere_radius = planet_radius * 2.;

	vec3 cam_dir = VIEW;

	vec3 sphere_origin = planet_position;

	float depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float max_depth = length(view.xyz);

	//vec2 rsi = ray_sphere_intersect(cam_pos, cam_dir, planet_position, atmosphere_radius, max_depth);
	vec2 rsi = raySphere2(planet_position, atmosphere_radius, cam_pos, cam_dir);

	float dist_through_atmosphere = min(rsi.y, max_depth - rsi.x);

	//ALPHA = smoothstep(0.0, 1.0, (rsi.y-rsi.x) * thickness_scale);
	ALPHA = clamp((rsi.y-rsi.x)*thickness_scale, 0.0, 1.0);
	if(dist_through_atmosphere > 0.) {
		const float epsilon = 0.0001;
		vec3 point_in_atmosphere = cam_pos + cam_dir * rsi.x - epsilon;
		float light = calculate_light(point_in_atmosphere, sun_direction, dist_through_atmosphere - epsilon * 2.);
		ALBEDO = atmosphere_color * (1.-light) + light;
		//ALBEDO = vec3(rsi.x, rsi.y, 0.);

	}
	else{
		ALBEDO = atmosphere_color;
	}
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
