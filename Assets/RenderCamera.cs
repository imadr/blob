using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderCamera : MonoBehaviour
{
    public bool fullscreen = false;
    public Button fullscreen_button;
    public Blob blob;

    void Start(){
        fullscreen_button.onClick.AddListener(ToggleFullscreen);
    }

    void ToggleFullscreen(){
        fullscreen = !fullscreen;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest){
        if(fullscreen){
            if(blob.render_texture != null) Graphics.Blit(blob.render_texture, dest,
                new Vector2((float)src.width/src.height, 1),
                new Vector2(-(float)src.width/src.height/2+0.5f, 0));
        }
        else{
            Graphics.Blit(src, dest);
        }
    }
}
