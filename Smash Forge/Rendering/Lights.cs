using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;
using SALT.Graphics;
using System.Diagnostics;
using System.Globalization;


namespace Smash_Forge
{
    public class ColorContainer
    {
        //Store the color in both forms so that it only needs recalculation when changed
        private Vector3 hsv;
        private Vector3 rgb;

        public Vector3 HSV
        {
            get {return hsv;}
            set
            {
                hsv = value;
                float r,g,b;
                ColorTools.HSV2RGB(hsv[0], hsv[1], hsv[2], out r, out g, out b);
                rgb = new Vector3(r,g,b);
            }
        }
        public Vector3 RGB
        {             
            get {return rgb;}
            set
            {
                rgb = value;
                float h,s,v;
                ColorTools.RGB2HSV(rgb[0], rgb[1], rgb[2], out h, out s, out v);
                hsv = new Vector3(h,s,v);
            }
        }

        public ColorContainer()
        {
            hsv = new Vector3(0f,0f,1f);
            rgb = new Vector3(1f,1f,1f);
        }
    }

    public class AmbientLight
    {
        public float enabled; //Not sure what this actually is given that it's a float
        public ColorContainer color;

        public AmbientLight()
        {
            enabled = 1f;
            color = new ColorContainer();
        }
        public string ToString()
        {
            return $"[{enabled}],[{color.HSV.ToString()}]";
        }
    }

    public class Hemisphere
    {
        public ColorContainer color;
        public float angle;
        public Vector3 vectorAngle
        {             
            get
            {
                Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, angle * ((float)Math.PI / 180f));
                return Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
            }
        }

        public Hemisphere()
        {
            color = new ColorContainter();
            angle = 0f;
        }
        public string ToString()
        {
            return $"[{color.HSV.ToString()}],[{angle}]";
        }
    }

    public class HemisphereLight
    {
        public Hemisphere sky;
        public Hemisphere ground;

        public HemisphereLight()
        {
            sky = new Hemisphere();
            ground = new Hemisphere();
        }
        public string ToString()
        {
            return $"{sky.ToString()}\n{ground.ToString()}";
        }
    }

    public class DirectionalLight
    {
        public uint unk;
        public uint enabled;
        public ColorContainer color;
        public Vector3 eulerAngle;
        public Vector3 vectorAngle
        {             
            get
            {
                Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, eulerAngle.X * ((float)Math.PI / 180f))
                 * Matrix4.CreateFromAxisAngle(Vector3.UnitY, eulerAngle.Y * ((float)Math.PI / 180f))
                 * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, eulerAngle.Z * ((float)Math.PI / 180f));

                return Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
            }
        }

        public DirectionalLight()
        {
            unk = 1;
            enabled = 1;
            color = new ColorContainer();
            eulerAngle = new Vector3(0f,0f,0f);
        }
        public string ToString()
        {
            return $"[{enabled}],[{eulerAngle}],[{color.HSV.ToString()}]";
        }
    }

    public class Fog
    {
        public ColorContainer color;

        public Fog()
        {
            color = new ColorContainer();
        }
        public string ToString()
        {
            return $"[{color.HSV.ToString()}]";
        }
    }

    public class LightSet
    {
        public List<DirectionalLight> lights;
        public Fog fog;

        public LightSet()
        {
            lights = new List<DirectionalLight>();
            for (int i = 0; i < 4; i++)
                lights.Add(new DirectionalLight());
            fog = new Fog();
        }
    }

    public class LightSetParam
    {
        public HemisphereLight fresnelLight;
        public AmbientLight fighterAmbientSky; //I'm guessing that these two are sky and ground; need to put them together if it's the case
        public AmbientLight fighterAmbientGround;
        public DirectionalLight specularLight; //Need to know what this actually is
        public List<LightSet> lightSets;

        public LightSetParam()
        {
            fresnelLight = new HemisphereLight();
            fighterAmbientSky = new AmbientLight();
            fighterAmbientGround = new AmbientLight();
            specularLight = new DirectionalLight();
            lightSets = new List<LightSet>();
            for (int i = 0; i < 17; i++)
                lightSets.Add(new LightSet());
        }
        public LightSetParam(ParamFile lightSetParamFile) : this()
        {
            //Fresnel
            fresnelLight.sky.color.HSV[0] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 8);
            fresnelLight.sky.color.HSV[1] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 9);
            fresnelLight.sky.color.HSV[2] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 10);

            fresnelLight.ground.color.HSV[0] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 11);
            fresnelLight.ground.color.HSV[1] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 12);
            fresnelLight.ground.color.HSV[2] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 13);

            fresnelLight.sky.angle = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 14);
            fresnelLight.ground.angle = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 15);

            //Ambients
            fighterAmbientSky.enabled = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 28);
            fighterAmbientSky.color.HSV[0] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 29);
            fighterAmbientSky.color.HSV[1] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 30);
            fighterAmbientSky.color.HSV[2] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 31);

            fighterAmbientGround.enabled = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 32);
            fighterAmbientGround.color.HSV[0] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 33);
            fighterAmbientGround.color.HSV[1] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 34);
            fighterAmbientGround.color.HSV[2] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 0, 0, 35);

            //Lights and fog
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    lightSets[i].lights[j].unk = (uint)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 0);
                    lightSets[i].lights[j].enabled = (uint)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 1);

                    lightSets[i].lights[j].color.HSV[0] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 2);
                    lightSets[i].lights[j].color.HSV[1] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 3);
                    lightSets[i].lights[j].color.HSV[2] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 4);

                    lightSets[i].lights[j].eulerAngle = new Vector3();
                    lightSets[i].lights[j].eulerAngle.X = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 5);
                    lightSets[i].lights[j].eulerAngle.Y = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 6);
                    lightSets[i].lights[j].eulerAngle.Z = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 1, (i*4)+j, 7);
                }
                lightSets[i].fog.color.HSV[0] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 2, i, 0);
                lightSets[i].fog.color.HSV[1] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 2, i, 1);
                lightSets[i].fog.color.HSV[2] = (float)RenderTools.GetValueFromParamFile(lightSetParamFile, 2, i, 2);
            }
        }
    }

    public class AreaLight
    {
        public string id;
        public Vector3 pos;
        public Vector3 scale;
        public Vector3 rot;
        public ColorContainer col_ceiling;
        public ColorContainer col_ground;

        public AreaLight()
        {
            id = "";
            pos = new Vector3(0f);
            scale = new Vector3(1f);
            rot = new Vector3(0f);
            col_ceiling = new ColorContainer();
            col_ground = new ColorContainer();
        }
        public AreaLight(XMBEntry entry) : this()
        {
            for (int i = 0; i < entry.Expressions.Count; i++)
            {
                string expression = entry.Expressions[i];

                string[] sections = expression.Split('=');
                string name = sections[0];
                string[] values = sections[1].Split(',');

                if (name.Contains("id"))
                {
                    id = values[0];
                }
                else if (name.Contains("pos") || name.Contains("scale") || name.Contains("rot") || name.Contains("col_ceiling") || name.Contains("col_ground"))
                {
                    float x,y,z;
                    float.TryParse(values[0], out x);
                    float.TryParse(values[1], out y);
                    float.TryParse(values[2], out z);

                    if (name.Contains("pos"))
                        pos = new Vector3(x,y,z);
                    else if (name.Contains("scale"))
                        scale = new Vector3(x,y,z);
                    else if (name.Contains("rot"))
                        rot = new Vector3(x,y,z);
                    else if (name.Contains("col_ceiling"))
                        col_ceiling.rgb = new Vector3(x,y,z);
                    else if (name.Contains("col_ground"))
                        col_ground.rgb = new Vector3(x,y,z);
                }
            }
        }
    }

    public class LightMap
    {
        public string id;
        public Vector3 pos;
        public Vector3 scale;
        public Vector3 rot;
        public uint texture_index;
        public uint texture_addr;

        public LightMap()
        {
            id = "";
            pos = new Vector3(0f);
            scale = new Vector3(1f);
            rot = new Vector3(0f);
            texture_index = 0x10080000;
            texture_addr = 0;
        }
        public LightMap(XMBEntry entry) : this()
        {
            for (int i = 0; i < entry.Expressions.Count; i++)
            {
                string expression = entry.Expressions[i];

                string[] sections = expression.Split('=');
                string name = sections[0];
                string[] values = sections[1].Split(',');

                if (name.Contains("id"))
                {
                    id = values[0];
                }
                else if (name.Contains("texture_index"))
                {
                    string index = values[0].Trim();
                    if (index.StartsWith("0x"))
                        index = index.Substring(2);
                    uint.TryParse(index, NumberStyles.HexNumber, null, out texture_index);
                }
                else if (name.Contains("texture_addr"))
                {
                    uint.TryParse(values[0], out texture_addr);
                }
                else if (name.Contains("pos") || name.Contains("scale") || name.Contains("rot"))
                {
                    float x,y,z;
                    float.TryParse(values[0], out x);
                    float.TryParse(values[1], out y);
                    float.TryParse(values[2], out z);

                    if (name.Contains("pos"))
                        pos = new Vector3(x,y,z);
                    else if (name.Contains("scale"))
                        scale = new Vector3(x,y,z);
                    else if (name.Contains("rot"))
                        rot = new Vector3(x,y,z);
                }
            }
        }
    }


    public class Lighting
    {
        public LightSetParam lightSetParam;
        public List<AreaLight> areaLights;
        public List<LightMap> lightMaps;

        public Lighting()
        {
            lightSetParam = new LightSetParam();
            areaLights = new List<AreaLight>();
            lightMaps = new List<LightMap>();
        }

        public void CreateAreaLightsFromXMB(XMBFile xmb)
        {
            if (xmb == null) return;

            foreach (XMBEntry entry in xmb.Entries)
                foreach (XMBEntry lightEntry in entry.Children)
                    areaLights.Add(new AreaLight(lightEntry));
        }
        public void CreateLightMapsFromXMB(XMBFile xmb)
        {
            if (xmb == null) return;

            foreach (XMBEntry entry in xmb.Entries)
                foreach (XMBEntry lightMapEntry in entry.Children)
                    lightMaps.Add(new LightMap(lightMapEntry));
        }
    }
}
