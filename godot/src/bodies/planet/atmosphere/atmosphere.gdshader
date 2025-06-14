shader_type spatial;
render_mode unshaded, depth_test_disabled;

// color of the shadow cast by the atmosphere, default is black
uniform vec3 shadow_color : source_color = vec3(0.0);
uniform vec3 planet_position;
uniform vec3 sun_direction;
// scales the thickness of the atmosphere (controls glow and haze appearance)
uniform float thickness_scale : hint_range(0.0, 100.0, 0.001);
uniform sampler2D depth_texture : hint_depth_texture;
uniform float planet_radius;
uniform float density_falloff : hint_range(-10.0, 20.0, 0.1);
uniform int num_in_scattering_points : hint_range(0, 100, 1);
uniform int num_optical_depth_points : hint_range(0, 100, 1);
uniform vec3 wavelengths;
uniform float scattering_strenght: hint_range(0.0, 100.0, 0.01);
uniform float atmosphere_scale: hint_range(1.0, 2.0, 0.05);
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

varying vec3 cam_pos;
varying float atmosphere_radius;
varying vec3 sunDirView;
varying vec3 scattering_coefficients;

// FOR LOD
varying float CLOSE_THRESHOLD;
varying float MEDIUM_THRESHOLD;
varying float FAR_THRESHOLD;


vec2 raySphere(vec3 sphereCenter, float sphereRadius, vec3 rayOrigin, vec3 rayDir, float maxDepth){
	vec3 offset = rayOrigin - sphereCenter;
	float a = 1.;
	float b = 2. * dot(offset, rayDir);
	float c = dot(offset,offset) - sphereRadius * sphereRadius;
	float d = b * b - 4. * a * c;

	if(d > 0.){
		float s = sqrt(d);
		float dstToSphereNear = max(0, (-b-s) / (2. * a));
		float dstToSphereFar = (-b + s) / (2. * a);

		if (dstToSphereFar > 0.){
			return vec2(dstToSphereNear, min(dstToSphereFar,maxDepth));
		}
	}
	return vec2(1e20, 0.);
}

float calculate_dist_to_camera(){
	return length(cam_pos - planet_position);
}

int get_LOD_value(float dist_to_camera, int value) {
	float lod = 1.0; // full detail by default

	// Smooth transition between CLOSE_THRESHOLD and MEDIUM_THRESHOLD
	if (dist_to_camera < CLOSE_THRESHOLD) {
	    lod = 1.0;
	} else if (dist_to_camera < MEDIUM_THRESHOLD) {
	    float t = smoothstep(CLOSE_THRESHOLD, MEDIUM_THRESHOLD, dist_to_camera);
	    lod = mix(1.0, 0.5, t); // smoothly blend from 1.0 to 0.5
	} else {
	    float t = smoothstep(MEDIUM_THRESHOLD, MEDIUM_THRESHOLD + 100.0, dist_to_camera);
	    lod = mix(0.5, 0.25, t); // smoothly blend from 0.5 to 0.25
	}

	return int(ceil(lod * float(value)));
}

float density_at_point(vec3 density_sample_point){
	float height_above_surface = length(density_sample_point - planet_position) - planet_radius;
	float height01 = height_above_surface / (atmosphere_radius - planet_radius);
	float local_density = exp(-height01 * density_falloff) * (1. - height01);
	return local_density;
}

float optical_depth(vec3 ray_origin, vec3 ray_dir, float ray_length, float cam_dist){
	int lod_num_optical_depth_points = get_LOD_value(cam_dist, num_optical_depth_points);
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

vec3 calculate_light(vec3 ray_origin, vec3 ray_dir, float ray_length, vec3 original_col){
	float cam_dist = calculate_dist_to_camera();
	int lod_num_in_scattering_points = get_LOD_value(cam_dist, num_in_scattering_points);
	vec3 in_scatter_point = ray_origin;
	float step_size = ray_length / float(num_in_scattering_points);
	step_size = min(step_size, 10.);
	vec3 in_scattered_light = vec3(0.);
	float view_ray_optical_depth = 0.;

	for(int i = 0; i < lod_num_in_scattering_points; i++){
		float sun_ray_length = raySphere(planet_position, atmosphere_radius, in_scatter_point, -sunDirView, 10000.).y;
		float sun_ray_optical_depth = optical_depth(in_scatter_point, -sunDirView, sun_ray_length, cam_dist);
		view_ray_optical_depth = optical_depth(in_scatter_point, -ray_dir, step_size*float(i), cam_dist);
		vec3 transmittance = exp(-(sun_ray_optical_depth + view_ray_optical_depth) * scattering_coefficients);
		float local_density = density_at_point(in_scatter_point);

		in_scattered_light += local_density * transmittance * scattering_coefficients * step_size;
		in_scatter_point += ray_dir * step_size;
	}
	float original_col_transmittance = exp(-view_ray_optical_depth);
	return original_col * original_col_transmittance + in_scattered_light;
}


vec3 calculate_wavelengths() {
	float scatterR = pow(400. / wavelengths.x, 4) * scattering_strenght;
	float scatterG = pow(400. / wavelengths.y, 4) * scattering_strenght;
	float scatterB = pow(400. / wavelengths.z, 4) * scattering_strenght;
	return vec3(scatterR, scatterG, scatterB);
}

void vertex() {
	cam_pos = MODELVIEW_MATRIX[3].xyz;
}

void fragment() {
	CLOSE_THRESHOLD = 100. * planet_radius;
	MEDIUM_THRESHOLD = 200. * planet_radius;
	FAR_THRESHOLD = 300. * planet_radius;

	scattering_coefficients = calculate_wavelengths();

	// scale the atmosphere radius
	atmosphere_radius = planet_radius * atmosphere_scale;

	// transform sun direction to view space
	sunDirView = normalize(VIEW_MATRIX * vec4(sun_direction,0.0)).xyz;

	// camera direction in viewspace
	vec3 cam_dir = VIEW;

	vec3 sphere_origin = planet_position;

	// get the view depth, see https://docs.godotengine.org/en/stable/tutorials/shaders/advanced_postprocessing.html#depth-texture
	float depth = texture(depth_texture, SCREEN_UV).x;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    view.xyz /= view.w;
    float max_depth = length(view.xyz);

	// calculate raysphere intersection
	vec2 rsi = raySphere(planet_position, atmosphere_radius, cam_pos, cam_dir, max_depth);

	float dist_through_atmosphere = min(rsi.y, max_depth - rsi.x);
	// if ray misses atmosphere, dont colour it
	if(dist_through_atmosphere > 0.) {
		vec3 scene_col = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
		const float epsilon = 0.0001;
		vec3 point_in_atmosphere = cam_pos + cam_dir * (rsi.x + epsilon);
		vec3 light = calculate_light(point_in_atmosphere, cam_dir, dist_through_atmosphere - epsilon * 2., scene_col);
		ALBEDO = light;
	}

	ALPHA = clamp((rsi.y-rsi.x)*thickness_scale, 0.0, 1.0);
	float fog_factor = 1.0 - exp(-0.001 * (rsi.y - rsi.x) * thickness_scale);
	ALPHA = clamp(fog_factor, 0.0, 1.0);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
