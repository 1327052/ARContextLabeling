using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace Assets
{
    class Attraction : Component, IComparable
    {
        public String Name { get; set; }

        public String JapaneseName { get; set; }

        public double Latituide { get; set; }

        public double Longitude { get; set; }

        public int WaitingTime { get; set; }

        public String ImagePath { get; set; }

        public Image AttractionBackground { get; set; }

        public float Distance { get; set; }

        public int currentLoD { get; set; }

        public int goalLoD { get; set; }

        public Canvas ParentCanvas { get; set; }

        public Vector3 goalPosition { get; set; }

        public Vector3 speed { get; set; }

        public Vector3 initialPosition { get; set; }
        public Vector3 helpPosition { get; set; }
        public GameObject CanvasGO { get; set; }

        public Text AttractionText { get; set; }

        public Text ShortText { get; set; }

        public RawImage WaitImage { get; set; }
        public RawImage AttractionImage { get; set; }

        public RawImage CubeImage { get; set; }

        public bool isAreaExpanded { get; set; }

        public Transform SmallCubeTransform { get; set; }
        public Transform CubeTransform { get; set; }

        public float lodChangeCompletion { get; set; }

        public SpriteRenderer svgRenderer { get; set; }
        public SpriteRenderer svgBackRenderer { get; set; }
                


        //public LoD currentLOD;
        public enum LoD { HIGH, MID, LOW };

        public Attraction(string name, string japaneseName, double latituide, double longitude, int waitingTime, String imagePath, Image attractionBackground, GameObject canvasGO)
        {
            Name = name;
            JapaneseName = japaneseName;
            Latituide = latituide;
            Longitude = longitude;
            WaitingTime = waitingTime;
            ImagePath = imagePath;

            isAreaExpanded = true;

            //Texture2D attrImage = Resources.Load(imagePath) as Texture2D;
            //attractionImage.texture = attrImage;

            AttractionBackground = attractionBackground;
            currentLoD = 3;
            CanvasGO = canvasGO;

            currentLoD = (int)LoD.MID;
            goalLoD = (int)LoD.MID;
            lodChangeCompletion = 0.0f;
            
            AttractionText = AttractionBackground.transform.GetComponentsInChildren<Text>()[0];
            ShortText = AttractionBackground.transform.GetComponentsInChildren<Text>()[1];
            WaitImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[0];
            AttractionImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[1];
            CubeImage = AttractionBackground.transform.GetComponentsInChildren<RawImage>()[2];

            CubeTransform = AttractionBackground.transform.GetChild(3);

            SmallCubeTransform = AttractionBackground.transform.GetChild(4);

            var shortTextColor = ShortText.color;
            shortTextColor.a = 0.0f;
            ShortText.color = shortTextColor;

            /*if (name == "Peter Pan’s Flight")
            {
                Vector3 prevPosition = AttractionImage.transform.position;
                //AttractionBackground.transform.DetachChildren();
                AttractionImage.transform.parent = null;

                AttractionImage.transform.SetParent(CanvasGO.transform);
                AttractionImage.transform.position = prevPosition;
            }*/

        }

        public void move(Vector3 direction, bool rotate)
        {
            AttractionBackground.transform.position += direction;
            //CanvasGO.transform.LookAt(new Vector3(0, 0, 0));
            //CanvasGO.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

            //CanvasGO.GetComponent<Canvas>().transform.LookAt(new Vector3(0, 0, 0));
            //CanvasGO.GetComponent<Canvas>().transform.Rotate(new Vector3(0, 1, 0), 180.0f);

            if (Name == "Peter Pan’s Flight")
            {
                //AttractionText.transform.position += direction;
                //WaitImage.transform.position += direction;
                //AttractionImage.transform.position += direction;
                //SmallCubeTransform.transform.position += direction;                
            } /*else
            {
                CanvasGO.transform.position += direction;
            }*/


            if (rotate)
            {
                AttractionBackground.transform.LookAt(new Vector3(0, AttractionBackground.transform.position.y, 0));
                AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                if (Name == "Peter Pan’s Flight")
                {
                    //AttractionText.transform.LookAt(new Vector3(0, AttractionBackground.transform.position.y, 0));
                    //AttractionText.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                    //WaitImage.transform.LookAt(new Vector3(0, WaitImage.transform.position.y, 0));
                    //WaitImage.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                    //AttractionImage.transform.LookAt(new Vector3(0, AttractionImage.transform.position.y, 0));
                    //AttractionImage.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                    //SmallCubeTransform.transform.LookAt(new Vector3(0, SmallCubeTransform.transform.position.y, 0));
                    //SmallCubeTransform.transform.Rotate(new Vector3(0, 1, 0), 180.0f);
                }
            }

        }


        public void moveEase(float easeValue)
        {
            //AttractionBackground.transform.position = 
            

        }

        public void moveEase(float easeValue, EasingFunction.Ease easeType)
        {
            if (easeValue > 1.0f)
            {
                easeValue = 1.0f;
            }

            /* (easeType == EasingFunction.Ease.EaseInOutCubic)
            {
                AttractionBackground.transform.position = initialPosition + (goalPosition - initialPosition) * EasingFunction.EaseInOutCubic(0, 1, easeValue);
            } else if (easeType == EasingFunction.Ease.EaseInOutQuad)
            {
                AttractionBackground.transform.position = initialPosition + (goalPosition - initialPosition) * EasingFunction.EaseInOutQuad(0, 1, easeValue);
            }*/

            AttractionBackground.transform.position = initialPosition + (goalPosition - initialPosition) * EasingFunction.GetEasingFunction(easeType).Invoke(0, 1, easeValue);

            AttractionBackground.transform.LookAt(new Vector3(0, AttractionBackground.transform.position.y, 0));
            AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

            //AttractionBackground.transform.position = initialPosition + (goalPosition - initialPosition) * easeValue;
        }

        public void hideAttraction(float alpha)
        {   
            var waitImageColor = WaitImage.color;
            waitImageColor.a = alpha;

            WaitImage.color = waitImageColor;            
            if (/*goalLoD == (int)LoD.LOW ||*/ currentLoD == (int)LoD.LOW)
            {
                var textTransColor = AttractionText.color;
                textTransColor.a = alpha;
                AttractionText.color = textTransColor;

                var backColor = AttractionBackground.color;
                backColor.a = alpha;
                AttractionBackground.color = backColor;

                var attImageColor = AttractionImage.color;
                attImageColor.a = alpha;
                AttractionImage.color = attImageColor;

                //Icons
                var iconColor = svgBackRenderer.color;
                iconColor.a = alpha;
                svgBackRenderer.color = iconColor;
            }

            if (/*goalLoD == (int)LoD.MID ||*/ currentLoD == (int)LoD.MID)
            {
                var attImageColor = AttractionImage.color;
                attImageColor.a = alpha;
                AttractionImage.color = attImageColor;

                //Icons
                var iconColor = svgBackRenderer.color;
                iconColor.a = alpha;
                svgBackRenderer.color = iconColor;                
            }

            if (/*goalLoD == (int)LoD.HIGH || */currentLoD == (int)LoD.HIGH)
            {
                //var shortTextColor = ShortText.color;
                //shortTextColor.a = alpha;
                //ShortText.color = shortTextColor;

                //Icons    
                var smallIconColor = svgRenderer.color;
                smallIconColor.a = alpha;
                svgRenderer.color = smallIconColor;
            }                
        }

        public void shift(Vector3 shiftVector)
        {
            AttractionBackground.transform.position += shiftVector;

            if (Name == "Peter Pan’s Flight")
            {
                //AttractionText.transform.position += shiftVector;
                //WaitImage.transform.position += shiftVector;
                //AttractionImage.transform.position += shiftVector;
                //SmallCubeTransform.transform.position += shiftVector;
            }
        }

        public void shiftTo(Vector3 shiftVector)
        {
            AttractionBackground.transform.position = shiftVector;

            if (Name == "Peter Pan’s Flight")
            {
                //AttractionText.transform.position = shiftVector;
                //WaitImage.transform.position = shiftVector + new Vector3(0, 85, 0);
                //AttractionImage.transform.position = shiftVector + new Vector3(0, 85, 0);
                //SmallCubeTransform.transform.position = shiftVector + new Vector3(0, 85, 0);
            }

        }

        public bool isNotVisibleButThere()
        {
            if (AttractionBackground.GetComponentsInChildren<Component>()[16].GetComponent<MeshRenderer>().isVisible)
            {
                return true;
            }

            return false;
        }

        public bool isVisible()
        {
            /*if (AttractionBackground.GetComponentsInChildren<Component>()[16].GetComponent<MeshRenderer>().isVisible)
            {
                if (WaitImage.color.a >= 0.9f)
                {
                    return true;
                }              
            }            

            return false;*/
            if (AttractionBackground.GetComponentsInChildren<Component>()[16].GetComponent<MeshRenderer>().isVisible)
            {
                if (isAreaExpanded)
                {
                    return true;
                }
            }

            return false;

            /*if (currentLoD == 1)
                {
                    if (AttractionBackground.GetComponentsInChildren<Component>()[16].GetComponent<MeshRenderer>().isVisible)
                    {
                        return true;
                    }
                }
                else if (AttractionBackground.GetComponentsInChildren<Component>()[12].GetComponent<MeshRenderer>().isVisible)
                {
                    return true;
                }*/
            //}            
        }

        public bool isInside(Vector3[] corners, Vector3[] behindCorners, Vector3 normalVector)
        {
            Vector3 directionBottomLeft = behindCorners[0];
            Vector3 directionTopLeft = behindCorners[1];
            Vector3 directionTopRight = behindCorners[2];
            Vector3 directionBottomRight = behindCorners[3];

            directionBottomLeft.Normalize();
            directionTopLeft.Normalize();
            directionTopRight.Normalize();
            directionBottomRight.Normalize();

            Vector3[] directions = new Vector3[] { directionBottomLeft, directionTopLeft, directionTopRight, directionBottomRight };

            normalVector.Normalize();

            Vector3 u = (corners[0] - normalVector * 5) - (corners[0] + normalVector * 5);
            Vector3 v = (corners[0] - normalVector * 5) - (corners[3] - normalVector * 5);
            Vector3 w = (corners[0] - normalVector * 5) - (corners[1] - normalVector * 5);


            float minDistance = Mathf.Min(Mathf.Min(corners[0].magnitude, corners[1].magnitude), Mathf.Min(corners[2].magnitude, corners[3].magnitude));
            float maxDistance = Mathf.Max(Mathf.Max(corners[0].magnitude, corners[1].magnitude), Mathf.Max(corners[2].magnitude, corners[3].magnitude));

            float stepSize = 5;

            float uP1 = Vector3.Dot(corners[0] - normalVector * stepSize, u);
            float uP2 = Vector3.Dot(corners[0] + normalVector * stepSize, u);

            float vP1 = Vector3.Dot(corners[0] - normalVector * stepSize, v);
            float vP4 = Vector3.Dot(corners[3] - normalVector * stepSize, v);

            float wP1 = Vector3.Dot(corners[0] - normalVector * stepSize, w);
            float wP5 = Vector3.Dot(corners[1] - normalVector * stepSize, w);

            foreach (Vector3 direction in directions)
            {
                Vector3 point = direction * minDistance;

                bool collisionFound = false;

                while (point.magnitude <= maxDistance && !collisionFound)
                {
                    if (uP1 <= Vector3.Dot(u, point) && Vector3.Dot(u, point) <= uP2)
                    {
                        if (vP1 <= Vector3.Dot(v, point) && Vector3.Dot(v, point) <= vP4)
                        {
                            if (wP1 <= Vector3.Dot(w, point) && Vector3.Dot(w, point) <= wP5)
                            {
                                collisionFound = true;


                                /*directionTop = hitt1.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);
                                directionTop = (directionTop - Camera.main.transform.position).normalized;

                                hitPlane = new Plane(currentNormalVector, currentLabelPosition);

                                collisionRay = new Ray(Camera.main.transform.position, directionTop);
                                hitPlane.Raycast(collisionRay, out distValue);
                                endPoint = collisionRay.GetPoint(distValue);

                                if (endPoint.y > currentEndPoint.y)
                                {
                                    currentEndPoint = endPoint;
                                    currentHit = hitt1;
                                    isSthBlocking = true;
                                }*/
                            }
                        }
                    }

                    point += direction * stepSize * 2;
                }
            }


            return false;
        }

        public bool Equals(String japName)
        {
            return this.JapaneseName == japName;
        }

        public int CompareTo(object obj)
        {
            Attraction compAttraction = (Attraction)obj;

            /*if (compAttraction.Distance > this.Distance)
            {
                return -1;
            }
            else if (compAttraction.Distance < this.Distance)
            {
                return 1;
            }*/

            //float compDistance = new Vector2(); compAttraction.AttractionBackground.transform.position.magnitude;
            //float dist = AttractionBackground.transform.position.magnitude;

            float compDistance = new Vector2(compAttraction.AttractionBackground.transform.position.x, compAttraction.AttractionBackground.transform.position.z).magnitude;
            float dist = new Vector2(AttractionBackground.transform.position.x, AttractionBackground.transform.position.z).magnitude;

            if (compDistance > dist)
            {
                return -1;
            }
            else if (compDistance < dist)
            {
                return 1;
            }

            return 0;
        }

        public float GUIRectWithObject()
        {
            //Vector2 size = Vector2.Scale(rectTrans.rect.size, rectTrans.lossyScale);
            //return new Rect((Vector2)rectTrans.position - (size * 0.5f), size);

            RectTransform currentRectTransform = AttractionBackground.GetComponentsInChildren<RawImage>()[1].rectTransform;

            if (currentLoD == (int)LoD.LOW)
            {
                currentRectTransform = CubeImage.rectTransform;
            }


            Vector3 origPoint = new Vector3(0, 0);
            Vector3 upperPoint = new Vector3(0, 0);


            /*if (currentLoD == 1)
            {
                RectTransform currentRectTransformText = AttractionBackground.GetComponentInChildren<Text>().rectTransform;
                float margin = 40;

                origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
                upperPoint = Camera.main.WorldToScreenPoint(
                    currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x + currentRectTransformText.sizeDelta.x,
                    currentRectTransform.sizeDelta.y + currentRectTransformText.sizeDelta.y + margin)
                );
            }
            else
            {*/
            origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
            upperPoint = Camera.main.WorldToScreenPoint(
                currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x,
                currentRectTransform.sizeDelta.y)
            );
            //}

            return Math.Abs(upperPoint.y - origPoint.y);

            //Vector3 cen = go.GetComponent<Renderer>().bounds.center;
            //Vector3 ext = go.GetComponent<Renderer>().bounds.extents;

            //Bounds bounds = go.GetComponent<Renderer>().bounds;
            /*go.pos

            Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
            Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

            // Create rect in screen space and return - does not account for camera perspective
            return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);*/

            /*Vector2[] extentPoints = new Vector2[8]
                {
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
                 HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
                };
                Vector2 min = extentPoints[0];
                Vector2 max = extentPoints[0];
                foreach (Vector2 v in extentPoints)
                {
                    min = Vector2.Min(min, v);
                    max = Vector2.Max(max, v);
                }
                return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);*/
        }

        public float GUIRectWithObject(int tempLoD)
        {
            RectTransform currentRectTransform = AttractionBackground.GetComponentsInChildren<RawImage>()[1].rectTransform;

            if (tempLoD == (int)LoD.LOW)
            {
                currentRectTransform = CubeImage.rectTransform;
            }


            Vector3 origPoint = new Vector3(0, 0);
            Vector3 upperPoint = new Vector3(0, 0);

            /*if (currentLoD == 1)
            {
                RectTransform currentRectTransformText = AttractionBackground.GetComponentInChildren<Text>().rectTransform;
                float margin = 40;

                origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
                upperPoint = Camera.main.WorldToScreenPoint(
                    currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x + currentRectTransformText.sizeDelta.x,
                    currentRectTransform.sizeDelta.y + currentRectTransformText.sizeDelta.y + margin)
                );
            }
            else
            {*/
            origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
            upperPoint = Camera.main.WorldToScreenPoint(
                currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x,
                currentRectTransform.sizeDelta.y)
            );
            //}

            return Math.Abs(upperPoint.y - origPoint.y);

            //Vector2 size = Vector2.Scale(rectTrans.rect.size, rectTrans.lossyScale);
            //return new Rect((Vector2)rectTrans.position - (size * 0.5f), size);

            /*RectTransform currentRectTransform = AttractionBackground.GetComponentsInChildren<RawImage>()[1].rectTransform;
            Vector3 origPoint = new Vector3(0, 0);
            Vector3 upperPoint = new Vector3(0, 0);

            if (tempLoD == 1)
            {
                RectTransform currentRectTransformText = AttractionBackground.GetComponentInChildren<Text>().rectTransform;
                float margin = 40;

                origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
                upperPoint = Camera.main.WorldToScreenPoint(
                    currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x + currentRectTransformText.sizeDelta.x,
                    currentRectTransform.sizeDelta.y + currentRectTransformText.sizeDelta.y + margin)
                );
            }
            else
            {
                origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
                upperPoint = Camera.main.WorldToScreenPoint(
                    currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x,
                    currentRectTransform.sizeDelta.y)
                );
            }

            return Math.Abs(upperPoint.y - origPoint.y);    */
        }

        public RectTransformData GetScreenRectTransform(int tempLoD)
        {
            //Vector2 size = Vector2.Scale(rectTrans.rect.size, rectTrans.lossyScale);
            //return new Rect((Vector2)rectTrans.position - (size * 0.5f), size);

            RectTransform currentRectTransform = AttractionBackground.GetComponentsInChildren<RawImage>()[1].rectTransform;
            Vector3 origPoint = new Vector3(0, 0);
            Vector3 upperPoint = new Vector3(0, 0);

            RectTransformData returnData = new RectTransformData();

            //RectTransform returnRectTransform = currentRectTransform;

            /* if (tempLoD == 1)
             {
                 RectTransform currentRectTransformText = AttractionBackground.GetComponentInChildren<Text>().rectTransform;
                 float margin = 40;

                 origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
                 upperPoint = Camera.main.WorldToScreenPoint(
                     currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x + currentRectTransformText.sizeDelta.x,
                     currentRectTransform.sizeDelta.y + currentRectTransformText.sizeDelta.y + margin)
                 );
             }
             else
             {*/
            origPoint = Camera.main.WorldToScreenPoint(currentRectTransform.position);
            upperPoint = Camera.main.WorldToScreenPoint(
                currentRectTransform.position + new Vector3(currentRectTransform.sizeDelta.x,
                currentRectTransform.sizeDelta.y)
            );
            //}

            returnData.X = origPoint.x;
            returnData.Y = origPoint.y;
            returnData.Z = origPoint.z;

            returnData.Width = Math.Abs(upperPoint.x - origPoint.x);
            returnData.Height = Math.Abs(upperPoint.y - origPoint.y);

            return returnData;
        }

        public void changeTextTransparency(GameObject debugTextObject)
        {
            if (isAreaExpanded)
            {
                lodChangeCompletion += 0.04f;
                if (lodChangeCompletion > 1.0f) { lodChangeCompletion = 1.0f; }


                if (goalLoD == (int)LoD.LOW)
                {
                    if (currentLoD == (int)LoD.HIGH)
                    {
                        //var shortTextColor = ShortText.color;
                        //shortTextColor.a = (1.0f - lodChangeCompletion);
                        //ShortText.color = shortTextColor;

                        var attrImageColor = AttractionImage.color;
                        attrImageColor.a = lodChangeCompletion;
                        AttractionImage.color = attrImageColor;


                        //Icons
                        var iconColor = svgRenderer.color;
                        iconColor.a = (1.0f - lodChangeCompletion);
                        svgRenderer.color = iconColor;

                        var smallIconColor = svgBackRenderer.color;
                        smallIconColor.a = lodChangeCompletion;
                        svgBackRenderer.color = smallIconColor;
                    }

                    var textTransColor = AttractionText.color;
                    textTransColor.a = lodChangeCompletion;
                    AttractionText.color = textTransColor;

                    var backColor = AttractionBackground.color;
                    backColor.a = lodChangeCompletion;
                    AttractionBackground.color = backColor;
                }

                if (goalLoD == (int)LoD.HIGH)
                {
                    if (currentLoD == (int)LoD.LOW)
                    {
                        var textTransColor = AttractionText.color;
                        textTransColor.a = (1.0f - lodChangeCompletion);
                        AttractionText.color = textTransColor;

                        var backColor = AttractionBackground.color;
                        backColor.a = (1.0f - lodChangeCompletion); 
                        AttractionBackground.color = backColor;

                        var attrImageColor = AttractionImage.color;
                        attrImageColor.a = (1.0f - lodChangeCompletion); 
                        AttractionImage.color = attrImageColor;

                        

                        //Icons
                        var smallIconColor = svgBackRenderer.color;
                        smallIconColor.a = (1.0f - lodChangeCompletion);
                        svgBackRenderer.color = smallIconColor;
                    }
                    else
                    {
                        var attrImageColor = AttractionImage.color;
                        attrImageColor.a = (1.0f - lodChangeCompletion); 
                        AttractionImage.color = attrImageColor;

                        //Icons
                        var smallIconColor = svgBackRenderer.color;
                        smallIconColor.a = (1.0f - lodChangeCompletion);
                        svgBackRenderer.color = smallIconColor;
                    }

                    //var shortTextColor = ShortText.color;
                    //shortTextColor.a = lodChangeCompletion;
                    //ShortText.color = shortTextColor;

                    //Icons
                    var iconColor = svgRenderer.color;
                    iconColor.a = lodChangeCompletion;
                    svgRenderer.color = iconColor;

                }

                if (goalLoD == (int)LoD.MID)
                {
                    if (currentLoD == (int)LoD.LOW)
                    {
                        var textTransColor = AttractionText.color;
                        textTransColor.a = (1.0f - lodChangeCompletion);
                        AttractionText.color = textTransColor;

                        var backColor = AttractionBackground.color;
                        backColor.a = (1.0f - lodChangeCompletion);
                        AttractionBackground.color = backColor;
                    }
                    else
                    {
                        //var shortTextColor = ShortText.color;
                        //shortTextColor.a = (1.0f - lodChangeCompletion); ;
                        //ShortText.color = shortTextColor;

                        var attrImageColor = AttractionImage.color;
                        attrImageColor.a = lodChangeCompletion;
                        AttractionImage.color = attrImageColor;

                        //Icons
                        var iconColor = svgRenderer.color;
                        iconColor.a = (1.0f - lodChangeCompletion);
                        svgRenderer.color = iconColor;

                        var smallIconColor = svgBackRenderer.color;
                        smallIconColor.a = lodChangeCompletion;
                        svgBackRenderer.color = smallIconColor;
                    }

                }

                /*float transparency = AttractionText.color.a;

                if (goalLoD == (int)LoD.LOW)
                {
                    transparency += 0.04f;
                }
                else
                {
                    transparency -= 0.04f;
                }

                if (transparency < 0.0f) { transparency = 0.0f; }
                if (transparency > 1.0f) { transparency = 1.0f; }

                var textTransColor = AttractionText.color;
                textTransColor.a = transparency;
                AttractionText.color = textTransColor;

                var backColor = AttractionBackground.color;
                backColor.a = transparency;
                AttractionBackground.color = backColor;*/
            }
        }
    }
}
