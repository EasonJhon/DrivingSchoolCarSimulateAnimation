using System.Collections;
using UnityEngine;

public class Glinting : MonoBehaviour
{
    public Color Color = new Color(1, 0, 1, 1); //闪烁颜色
    [Range(0, 1)] public float MaxBrightness = 0.5f; //最高发光亮度，取值范围[0,1],需大于最低发光亮度
    [Range(0, 1)] public float MinBrightness = 0.0f; //最低发光亮度，取值范围[0,1],需小于最高发光亮度
    [Range(0.2f, 30.0f)] public float Rate = 1; //闪烁频率，取值范围[0.2,30.0]

    private float h, s, v; //色调，饱和度，亮度
    private float deltaBrightness; //最低最高亮度差
    private Renderer renderer;
    private Material material;
    private readonly string keyword = "_EMISSION";
    private readonly string colorName = "_EmissionColor";
    private Coroutine glinting;

    private bool isFlashing;
    private bool increase = true;

    private void Awake()
    {
        renderer = gameObject.GetComponent<Renderer>();
        material = renderer.material;
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartGlinting();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFlashing)
        {

            if (increase)
            {
                v += deltaBrightness * Time.deltaTime * Rate;
                increase = v <= MaxBrightness;
            }
            else
            {
                v -= deltaBrightness * Time.deltaTime * Rate;
                increase = v <= MinBrightness;
            }
            material.SetColor(colorName,Color.HSVToRGB(h,s,v));
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            StartGlinting();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            StopGlinting();
        }
    }

    private void OnValidate()
    {
        if (MinBrightness is < 0 or > 1)
            MinBrightness = 0.0f;
        if (MaxBrightness is < 0 or > 1)
            MaxBrightness = 1;
        if (MinBrightness >= MaxBrightness)
        {
            MinBrightness = 0.0f;
            MaxBrightness = 1;
        }

        if (Rate is < 0.2f or > 30f)
            Rate = 1;

        deltaBrightness = MaxBrightness - MinBrightness;
        float tempV = 0;
        Color.RGBToHSV(Color, out h, out s, out tempV);
    }

    //开始闪烁
    public void StartGlinting()
    {
        material.EnableKeyword(keyword);
        Color.RGBToHSV(Color,out h, out s,out v);
        v = MinBrightness;
        deltaBrightness = MaxBrightness - MinBrightness;
        isFlashing = true;
        // if (glinting != null)
        // {
        //     StopCoroutine(glinting);
        // }
        //
        // glinting = StartCoroutine(IEGlingting());
    }
    
    //停止闪烁
    public void StopGlinting()
    {
        if (material.IsKeywordEnabled(keyword))
            material.DisableKeyword(keyword);
        isFlashing = false;
        
        // if (glinting != null)
        // {
        //     StopCoroutine(glinting);
        // }
    }
    
    private IEnumerator IEGlingting()
    {
        Color.RGBToHSV(Color,out h, out s,out v);
        v = MinBrightness;
        deltaBrightness = MaxBrightness - MinBrightness;
        bool increase = true;
        while (true)
        {
            if (increase)
            {
                v += deltaBrightness * Time.deltaTime * Rate;
                increase = v <= MaxBrightness;
            }
            else
            {
                v -= deltaBrightness * Time.deltaTime * Rate;
                increase = v <= MinBrightness;
            }
            material.SetColor(colorName,Color.HSVToRGB(h,s,v));
            yield return null;
        }
    }
}