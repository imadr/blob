using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blob : MonoBehaviour
{
    public ComputeShader shader;

    public RenderTexture render_texture;
    private RenderTexture smaller_texture;
    private RenderTexture render_texture_blurred;
    private RenderTexture render_texture_blurred_big;

    public RenderCamera render_camera;

    public struct Agent{
        public Vector2 position;
        public float rotation;
    }

    ComputeBuffer agents_buffer;
    Agent[] agents;

    public Material material;
    public int nb_agents = 100;
    public float move_speed = 50;
    public float diffusion_speed = 20;
    public float evaporation_speed = 0.1f;
    public float sensor_dist = 1.0f;
    public float sensor_size = 2.0f;
    public float turn_speed = 20.0f;

    public float dish_size = 200;

    public Button reset_button;

    public int render_texture_res = 3000;
    public int smaller_texture_scale = 100;

    private Vector2 center;

    void Start(){
        InitAgents();
        reset_button.onClick.AddListener(InitAgentsNextFrame);
    }

    private bool start_init = false;
    private bool finished_init = true;
    void InitAgentsNextFrame(){
        start_init = true;
    }

    void InitAgents(){
        if(render_texture != null) render_texture.Release();
        if(render_texture_blurred != null) render_texture_blurred.Release();
        if(render_texture_blurred_big != null) render_texture_blurred_big.Release();
        if(smaller_texture != null) smaller_texture.Release();

        center = new Vector2(
            render_texture_res/2,
            render_texture_res/2
        );

        render_texture = new RenderTexture(render_texture_res, render_texture_res, 24);
        render_texture.enableRandomWrite = true;
        render_texture.Create();

        render_texture_blurred_big = new RenderTexture(render_texture_res, render_texture_res, 24);
        render_texture_blurred_big.enableRandomWrite = true;
        render_texture_blurred_big.Create();

        render_texture_blurred = new RenderTexture(render_texture_res/smaller_texture_scale, render_texture_res/smaller_texture_scale, 24);
        render_texture_blurred.enableRandomWrite = true;
        render_texture_blurred.Create();
        smaller_texture = new RenderTexture(render_texture_res/smaller_texture_scale, render_texture_res/smaller_texture_scale, 24);
        smaller_texture.enableRandomWrite = true;
        smaller_texture.Create();

        agents = new Agent[nb_agents];
        for(int i = 0; i < nb_agents; i++){
            float radius = dish_size/3*Mathf.Sqrt(Random.Range(-1.0f, 1.0f));
            float angle = Random.Range(-10.0f, 10.0f)*2*Mathf.PI;
            Vector2 coord = new Vector2(radius*Mathf.Cos(angle), radius*Mathf.Sin(angle));
            agents[i] = new Agent(){
                position = center+coord,
                rotation = Random.Range(0, 360)*Mathf.PI/180
            };
        }

        shader.SetInt("nb_agents", nb_agents);
        if(agents_buffer != null) agents_buffer.Dispose();
        agents_buffer = new ComputeBuffer(agents.Length, sizeof(float)*3);
        finished_init = true;
    }

    void FixedUpdate(){
        if(!finished_init) return;
        if(render_texture == null) return;

        agents_buffer.SetData(agents);
        shader.SetInt("width", render_texture.width);
        shader.SetInt("height", render_texture.height);
        shader.SetFloat("move_speed", move_speed);
        shader.SetFloat("diffusion_speed", diffusion_speed);
        shader.SetFloat("evaporation_speed", evaporation_speed);
        shader.SetFloat("delta_time", Time.fixedDeltaTime);
        shader.SetFloat("sensor_dist", sensor_dist);
        shader.SetFloat("sensor_size", sensor_size);
        shader.SetFloat("turn_speed", turn_speed);
        shader.SetFloat("dish_size", dish_size);
        shader.SetVector("center", center);
        shader.SetBuffer(0, "agents", agents_buffer);
        shader.SetTexture(0, "output", render_texture);
        shader.Dispatch(0, nb_agents/64, 1, 1);
        agents_buffer.GetData(agents);

        shader.SetTexture(1, "output", render_texture);

        shader.Dispatch(1, render_texture.width/8+1, render_texture.height/8+1, 1);

        if(!render_camera.fullscreen){
            Graphics.Blit(render_texture, smaller_texture);
            shader.SetInt("width", smaller_texture.width);
            shader.SetInt("height", smaller_texture.height);
            shader.SetTexture(2, "output", smaller_texture);
            shader.SetTexture(2, "output_blurred", render_texture_blurred);
            shader.Dispatch(2, smaller_texture.width/8+1, smaller_texture.height/8+1, 1);

            Graphics.Blit(render_texture_blurred, render_texture_blurred_big);

            material.SetTexture("_Tex", render_texture);
            material.SetTexture("_TexBlurred", render_texture_blurred_big);
        }

        if(start_init){
            finished_init = false;
            start_init = false;
            InitAgents();
        }
    }

    void OnDestroy(){
        if(agents_buffer != null) agents_buffer.Dispose();
    }
}
