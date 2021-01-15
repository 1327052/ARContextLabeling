using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    class Area : Component, IEquatable<Area>
    {
        public String Name { get; set; }
        public String JapaneseName { get; set; }
        public int WaitingTime { get; set; }
        public String ImagePath { get; set; }
        public Image AttractionBackground { get; set; }
        public float Distance { get; set; }
        public int currentLoD { get; set; }
        public Canvas ParentCanvas { get; set; }
        public Vector3 goalPosition { get; set; }
        public Vector3 speed { get; set; }
        public Vector3 initialPosition { get; set; }
        public GameObject CanvasGO { get; set; }
        public Text AttractionText { get; set; }
        public RawImage WaitImage { get; set; }
        public RawImage AttractionImage { get; set; }
        public Transform SmallCubeTransform { get; set; }

        public Transform LargeCubeTransform { get; set; }

        public List<Attraction> SubAttractions { get; set; }

        public bool isExpanded { get; set; }

        public bool initTransition { get; set; }

        public float transparency { get; set; }

        public static Dictionary<string, string> TextureNames = new Dictionary<string, string>
        {
            { "Adventureland", "areas/Adventureland" },
            { "Toon Town", "areas/ToonTown" },
            { "Fantasyland", "areas/Fantasyland" },
            { "Tomorrowland", "areas/Tomorrowland" },
            { "Westernland", "areas/Westernland" }            
        };

        /*public static string[] TextureNames = new string[]
        {
            "areas/Adventureland"
        };*/

        public Area(string name/*, string japaneseName, int waitingTime, String imagePath, Image attractionBackground, GameObject canvasGO*/)
        {
            Name = name;

            isExpanded = false;

            SubAttractions = new List<Attraction>();

            initTransition = false;

            transparency = 0.0f;

            //JapaneseName = japaneseName;
            //WaitingTime = waitingTime;
            //ImagePath = imagePath;

            //Texture2D attrImage = Resources.Load(imagePath) as Texture2D;
            //attractionImage.texture = attrImage;

            //AttractionBackground = attractionBackground;
            //currentLoD = 3;
            //CanvasGO = canvasGO;

            //AttractionText = AttractionBackground.transform.GetComponentInChildren<Text>();

            //WaitImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[0];
            //AttractionImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[1];


            //SmallCubeTransform = AttractionBackground.transform.GetChild(4);

            /*if (name == "Peter Pan’s Flight")
            {
                Vector3 prevPosition = AttractionImage.transform.position;
                //AttractionBackground.transform.DetachChildren();
                AttractionImage.transform.parent = null;

                AttractionImage.transform.SetParent(CanvasGO.transform);
                AttractionImage.transform.position = prevPosition;
            }*/
        }

        public void setValues()
        {
            AttractionText = AttractionBackground.transform.GetComponentInChildren<Text>();

            WaitImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[0];
            AttractionImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[1];

            LargeCubeTransform = AttractionBackground.transform.GetChild(3);
            SmallCubeTransform = AttractionBackground.transform.GetChild(4);

            var smallIconColor = AttractionBackground.GetComponentsInChildren<SpriteRenderer>()[1].color;
            smallIconColor.a = 0.0f;
            AttractionBackground.GetComponentsInChildren<SpriteRenderer>()[1].color = smallIconColor;


            /* WaitImage.transform.position = AttractionBackground.transform.position - new Vector3(0, 85, -4.5f);
             AttractionImage.transform.position = AttractionBackground.transform.position - new Vector3(0, 85, -1.5f);
             SmallCubeTransform.position = AttractionBackground.transform.position - new Vector3(0, 85, -4);
             LargeCubeTransform.position = AttractionBackground.transform.position - new Vector3(0, 60, -5);*/

            Text ShortText = AttractionBackground.transform.GetComponentsInChildren<Text>()[1];

            var shortTextColor = ShortText.color;
            shortTextColor.a = 0.0f;
            ShortText.color = shortTextColor;

            /*RawImage subImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[3];
            RawImage newSubImage;
            Texture2D subImageTexture;

            int subImagesCount = 5;
            Vector3 startPos = subImage.transform.position - new Vector3(10 * (subImagesCount / 2), 0, 0);
            for (int u = 0; u < subImagesCount; u++)
            {
                newSubImage = RawImage.Instantiate(subImage);

                var subImageColor = newSubImage.color;
                subImageColor.a = 1.0f;
                newSubImage.color = subImageColor;


                //subImageTexture = Resources.Load("icons/" + (100 - int.Parse(parts[6]))) as Texture2D;
                subImageTexture = Resources.Load("icons/" + (u * 20)) as Texture2D;
                newSubImage.texture = subImageTexture;


                newSubImage.transform.SetParent(AttractionBackground.transform);
                newSubImage.transform.position = startPos;
                startPos += new Vector3(10, 0, 0);
            }*/
        }

        public bool Equals(Area other)
        {
            if (this.Name == other.Name)
            {
                return true;
            }

            return false;
        }

        public void move(Vector3 vector3)
        {
            AttractionBackground.transform.position += vector3;
            AttractionBackground.transform.LookAt(new Vector3(0, AttractionBackground.transform.position.y, 0));
            AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);
        }

        public void hideAttractions()
        {
            foreach (Attraction subAttraction in SubAttractions)
            {
                subAttraction.hideAttraction(0.0f);
            }
        }
        public void setAttrTransparency(float alpha)
        {
            foreach (Attraction subAttraction in SubAttractions)
            {
                subAttraction.hideAttraction(alpha);
            }
        }

        public void Expand()
        {
            setAttrTransparency(1.0f);
            setTransparency(0.0f);
        }

        public void ignoreCollision(bool b)
        {
            if (b)
            {
                SmallCubeTransform.gameObject.layer = 9;
            } else
            {
                SmallCubeTransform.gameObject.layer = 0;
            }
        }

        private void setTransparency(float alpha)
        {
            var attImageColor = AttractionImage.color;
            attImageColor.a = alpha;
            AttractionImage.color = attImageColor;

            var textTransColor = AttractionText.color;
            textTransColor.a = alpha;
            AttractionText.color = textTransColor;

            var backColor = AttractionBackground.color;
            backColor.a = alpha;
            AttractionBackground.color = backColor;

            var waitImageColor = WaitImage.color;
            waitImageColor.a = alpha;
            WaitImage.color = waitImageColor;

            foreach (RawImage subImage in AttractionBackground.GetComponentsInChildren<RawImage>())
            {
                waitImageColor = subImage.color;
                waitImageColor.a = alpha;
                subImage.color = waitImageColor;
            }

            //ignoreCollision(true);
        }

        public void prepareForTransition()
        {
            if (!isExpanded)
            {
                foreach (Attraction subAttraction in SubAttractions)
                {
                    subAttraction.AttractionBackground.transform.position = AttractionBackground.transform.position;
                    subAttraction.initialPosition = AttractionBackground.transform.position;

                    subAttraction.speed = (subAttraction.goalPosition - subAttraction.AttractionBackground.transform.position) / 100.0f;
                    //subAttraction.isAreaExpanded = !subAttraction.isAreaExpanded;
                }                
            } else
            {
                foreach (Attraction subAttraction in SubAttractions)
                {
                    subAttraction.helpPosition = subAttraction.goalPosition; //subAttraction.AttractionBackground.transform.position;
                    //subAttraction.AttractionBackground.transform.position = AttractionBackground.transform.position;
                    subAttraction.goalPosition = AttractionBackground.transform.position;
                    subAttraction.speed = (subAttraction.goalPosition - subAttraction.AttractionBackground.transform.position) / 100.0f;
                    //subAttraction.isAreaExpanded = !subAttraction.isAreaExpanded;
                }
            }

            isExpanded = !isExpanded;           
        }

        public void changeTransparency()
        {
            if (isExpanded)
            {
                transparency += 0.04f;

                if (transparency >= 1.0f)
                {
                    transparency = 1.0f;
                }
            } else
            {
                transparency -= 0.08f;

                if (transparency <= 0.0f)
                {
                    transparency = 0.0f;
                }
            }            

            setTransparency(1.0f - transparency);
            setAttrTransparency(transparency);
        }

        public void calculateValues()
        {
            float x = 0,z = 0;
            int wait = 0;

            foreach (Attraction subAttraction in SubAttractions)
            {
                x += subAttraction.AttractionBackground.transform.position.x;
                z += subAttraction.AttractionBackground.transform.position.z;
                wait += subAttraction.WaitingTime;
            }

            x /= SubAttractions.Count;
            z /= SubAttractions.Count;
            wait /= SubAttractions.Count;

            Texture waitTexture = Resources.Load("icons/" + (100 - wait)) as Texture2D;
            AttractionBackground.GetComponentsInChildren<RawImage>()[0].texture = waitTexture;

            AttractionBackground.transform.position = new Vector3(x, -270, z);

            AttractionBackground.transform.LookAt(new Vector3(0, AttractionBackground.transform.position.y, 0));
            AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

            //AttractionText.text += " " + wait;
        }

        public void moveLabelsToGoal()
        {
            foreach (Attraction subAttraction in SubAttractions)
            {
                subAttraction.AttractionBackground.transform.position = subAttraction.helpPosition;
            }
        }
    }
}
