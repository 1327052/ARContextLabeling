using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Threading;
using System.IO;
//using Assets;
using System.Collections.Generic;
using System;
using Assets;
using UnityEditor;

public class AugmentedScript : MonoBehaviour
{

    bool OclHandling = true;
    bool showAreas = false;
    bool showLOD = false; //if false, Middle LOD for each label


    //ForceOneLOD:
    bool showLowestLOD = false; //now Highest LOD
    bool showOnlyIcons = false; //now Lowest LOD
    bool showOnlyMid = false; 

    #region AugmentedScript variables

    GameObject canvasGameObject;

    SpriteRenderer thrillSVG, thrillBackSVG;
    SpriteRenderer adventureSVG, adventureBackSVG;
    SpriteRenderer childrenSVG, childrenBackSVG;

    private float originalLatitude;
    private float originalLongitude;
    private float currentLongitude;
    private float currentLatitude;
    private bool shiftReady = false;
    private bool firstOccluSolved = false;

    bool firstRegionChange = false;
    bool screenshots = false;

    bool showOnlyText = false;

    private bool first = true;

    private int prevTime;
    private int prevTimeLOD;

    private int numberOfShifts;
    private bool isUpdating = false;

    private bool distanceMoved = true;

    private Vector2 prevPosition = new Vector2(0, 0);
    private Vector2 currentPosition = new Vector2(0, 0);

    private GameObject distanceTextObject;

    private GameObject debugTextObject;

    private Image attractionBackground;
    private Text attractionText;
    private RawImage attractionImage;
    private Canvas attractionCanvas;

    private Vector3 prevDirection;
    private Vector2 prevLoDPosition;
    private bool firstAngle = true;

    private bool firstInit = true;
    private bool firstLowLoDFound = false;

    private double distance;

    bool initialAfterFiveSeconds = true;

    private bool setOriginalValues = true;

    private Vector3 targetPosition;
    private Vector3 originalPosition;

    private float speed = .1f;

    float startTime;

    private List<Attraction> attractions;
    private List<Area> areas;

    private List<int> visibleAttractions;

    private List<Texture2D> attrTextues;

    private Button forwardButton;
    private Button reverseButton;

    private bool forwardButtonDown = false;
    private bool reverseButtonDown = false;

    private Camera arCamera;

    Thread collisionThread;

    bool occlusionHandlingInProgress = false;

    private float detailSpace = 0.45f * (Screen.height / 2);
    private float overviewSpace = 0.3f * (Screen.height / 2);

    //private float detailSpace = 0.5f * (Screen.height / 2);
    //private float overviewSpace = 0.3f * (Screen.height / 2);

    private bool isFirstVisible = true;

    #endregion

    #region AugmentedScript OcclusionHandling
    IEnumerator OcclusionHandling()
    {
        bool running = true;

        while (running)
        {

            bool useoldLODs = false;
            isFirstVisible = true;


            //if (!forwardButtonDown && !reverseButtonDown && numberOfShifts == 0 && distanceMoved)
            //{

            if (/*!forwardButtonDown && !reverseButtonDown /*&& distanceMoved && */numberOfShifts == 0)
            { 
                try
                {
                    occlusionHandlingInProgress = true;

                    //try
                    //{

                    if (showAreas)
                    {
                        for (int i = 0; i < areas.Count; i++)
                        {

                            if (areas[i].AttractionBackground.transform.position.magnitude < 1450)
                            {
                                if (!areas[i].isExpanded)
                                {
                                    areas[i].ignoreCollision(true);
                                    areas[i].initTransition = true;
                                    //areas[i].isExpanded = true;

                                    foreach (Attraction subAttraction in areas[i].SubAttractions)
                                    {
                                        subAttraction.isAreaExpanded = true;
                                    }
                                }

                                /*if (!areas[i].isExpanded)
                                {
                                    areas[i].Expand();
                                }*/
                            }
                            if (areas[i].AttractionBackground.transform.position.magnitude > 1450)
                            {
                                if (areas[i].isExpanded)
                                {

                                    areas[i].ignoreCollision(false);
                                    areas[i].initTransition = true;
                                    //areas[i].isExpanded = true;

                                    foreach (Attraction subAttraction in areas[i].SubAttractions)
                                    {
                                        subAttraction.isAreaExpanded = false;
                                    }
                                }
                            }
                        }
                    }

                    int beforeTime = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

                    //debugTextObject.GetComponent<Text>().text = ((DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) - beforeHandlung) + " LLL";

                    attractions.Sort();

                    Rect rect;
                    RectTransformData currentRectTransform;



                    RaycastHit hitt1, hitt2, hitt3, hitt4;

                    Vector3[] currentCorners = new Vector3[4];
                    Vector3[] compareCorners = new Vector3[4];

                    Vector3 directionTop = new Vector3(), endPoint = new Vector3();
                    Vector3 currentLabelPosition;
                    Vector3 currentNormalVector;

                    Vector2 vectorOne, vectorTwo;

                    Vector3 compareNormalVector;

                    Plane hitPlane;
                    float distValue;
                    Ray collisionRay;

                    Vector3 currentEndPoint = new Vector3(0, -10000, 0);
                    RaycastHit currentHit = new RaycastHit();

                    string outMessage = "";
                    bool isSthBlocking = true;

                    float compareAngle;

                    Vector3 directionTopLeft, directionTopRight, directionBottomLeft, directionBottomRight;

                    /*attractions[0].AttractionBackground.transform.position =
                                    new Vector3(attractions[0].AttractionBackground.transform.position.x,
                                    0,
                                    attractions[0].AttractionBackground.transform.position.z);*/

                    //GameObject cube = attractions[0].AttractionBackground.GetComponentsInChildren<GameObject>();

                    //debugTextObject.GetComponent<Text>().text = attractions[0].AttractionBackground.gameObject.transform.GetChild(4) + "";// cube.name + " XXX";

                    attractions[0].initialPosition = attractions[0].AttractionBackground.transform.position;

                    
                    //Transform currentCube = attractions[0].SmallCubeTransform;
                    //currentCube.position = new Vector3(currentCube.position.x, 0, currentCube.position.z);


                    attractions[0].shiftTo(new Vector3(
                        attractions[0].AttractionBackground.transform.position.x,
                        0,
                        attractions[0].AttractionBackground.transform.position.z));

                    //JUST CHANGED ------------------------------------------------------------------------
                    //currentCube.position = attractions[0].AttractionBackground.transform.position + new Vector3(0, 85, 0);
                    //JUST CHANGED ------------------------------------------------------------------------

                    float rectHeight, occupiedHeight = 0.0f, heightLoD1, heightLoD2;

                    visibleAttractions.Clear();


                    bool updateLoD = false;

                    if (!firstAngle)
                    {
                        float cameraAngleDif = Vector3.Angle(prevDirection, Camera.main.transform.forward);

                        //debugTextObject.GetComponent<Text>().text += cameraAngleDif + " Grad";

                        if (useoldLODs)
                        {
                            if (cameraAngleDif <= 45)
                            {
                                updateLoD = false;
                            }
                            else if (cameraAngleDif >= 45 && DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - prevTimeLOD >= 2000)
                            {
                                updateLoD = true;
                                prevDirection = Camera.main.transform.forward;
                                prevTimeLOD = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                            } else if (cameraAngleDif >= 20 && DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - prevTimeLOD >= 6000)
                            {
                                updateLoD = true;
                                prevDirection = Camera.main.transform.forward;
                                prevTimeLOD = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                            }
                        }


                        if (!useoldLODs)
                        {
                            if ((currentPosition - prevLoDPosition).magnitude >= 200)
                            {
                                updateLoD = true;
                                prevLoDPosition = currentPosition;
                            }

                            if (!firstOccluSolved)
                            {
                                updateLoD = true;
                                firstOccluSolved = true;
                            }
                        }


                        /*if ((currentPosition - prevLoDPosition).magnitude <= 100)
                        {
                            updateLoD = false;
                            prevLoDPosition = currentPosition;
                        }*/

                    }
                    else
                    {
                        prevDirection = Camera.main.transform.forward;
                        firstAngle = false;
                        prevLoDPosition = currentPosition;
                        updateLoD = true;
                    }

                    Color textTransColor;
                    Color backColor;

                    int currentLODState = 0;

                    /*if (updateLoD)
                    {                     

                        attractions[0].goalLoD = (int)Attraction.LoD.LOW;

                        attractions[0].SmallCubeTransform.gameObject.layer = 0;
                        attractions[0].CubeTransform.gameObject.layer = 9;

                    }*/

                    //if (useoldLODs)
                    //{
                        for (int i = 1; i < attractions.Count; i++)
                        {

                            attractions[i].initialPosition = attractions[i].AttractionBackground.transform.position;
                            attractions[i].shift(new Vector3(0, 2000, 0));

                        if (useoldLODs)
                        {
                            if (updateLoD && showLOD)
                            {
                                attractions[i].goalLoD = (int)Attraction.LoD.MID;

                                attractions[i].SmallCubeTransform.gameObject.layer = 0;
                                attractions[i].CubeTransform.gameObject.layer = 9;
                            }
                        }

                        }
                    //}




                    //debugTextObject.GetComponent<Text>().text = "";
                    

                        /*if (updateLoD && showLOD &&!useoldLODs)
                        {
                            if (attractions[0].isNotVisibleButThere())
                            {
                                rectHeight = attractions[0].GUIRectWithObject();

                                attractions[0].goalLoD = (int)Attraction.LoD.LOW;

                                rectHeight = attractions[0].GUIRectWithObject();
                                occupiedHeight += rectHeight;


                                attractions[0].SmallCubeTransform.gameObject.layer = 9;
                                attractions[0].CubeTransform.gameObject.layer = 0;

                                firstLowLoDFound = true;
                            }
                        }*/

                    if (updateLoD && showLOD)// && useoldLODs)
                    {
                        attractions[0].goalLoD = (int)Attraction.LoD.LOW;
                        attractions[0].SmallCubeTransform.gameObject.layer = 9;
                        attractions[0].CubeTransform.gameObject.layer = 0;
                    }


                        for (int i = 1; i < attractions.Count; i++)
                    {

                        isSthBlocking = true;

                        /*if (attractions[i].Name != "Splash Mountain")
                        {*/
                        attractions[i].shiftTo(
                                    new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                    0,
                                    attractions[i].AttractionBackground.transform.position.z));
                        //}


                        string outString = "";

                        if (useoldLODs)
                        {
                            if (updateLoD && showLOD)
                            {
                                if (attractions[i].isNotVisibleButThere())
                                {
                                    rectHeight = attractions[i].GUIRectWithObject((int)Attraction.LoD.LOW);

                                    if (!firstLowLoDFound || occupiedHeight == 0.0f || (occupiedHeight < detailSpace && (occupiedHeight + rectHeight * 0.6) <= detailSpace) || (occupiedHeight + rectHeight) < detailSpace)
                                    {

                                        /*
                                        textTransColor = attractions[i].AttractionText.color;
                                        textTransColor.a = 1.0f;
                                        attractions[i].AttractionText.color = textTransColor;

                                        backColor = attractions[i].AttractionBackground.color;
                                        backColor.a = 1.0f;
                                        attractions[i].AttractionBackground.color = backColor;*/

                                        attractions[i].goalLoD = (int)Attraction.LoD.LOW;

                                        attractions[i].SmallCubeTransform.gameObject.layer = 9;
                                        attractions[i].CubeTransform.gameObject.layer = 0;
                                        firstLowLoDFound = true;
                                        //attractions[i].shift(new Vector3(0, 40, 0));

                                        //attractions[i].AttractionBackground.transform.LookAt(new Vector3(0, attractions[i].AttractionBackground.transform.position.y, 0));
                                        //attractions[i].AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                                        //attractions[i].AttractionBackground.transform.LookAt(new Vector3(0, attractions[i].AttractionBackground.transform.position.y, 0));
                                        //attractions[i].AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);
                                    }
                                    else if (occupiedHeight < (detailSpace + overviewSpace))
                                    {
                                        attractions[i].goalLoD = (int)Attraction.LoD.MID;
                                        rectHeight = attractions[i].GUIRectWithObject();
                                    }
                                    else
                                    {
                                        attractions[i].goalLoD = (int)Attraction.LoD.HIGH;
                                        rectHeight = attractions[i].GUIRectWithObject();
                                    }

                                    occupiedHeight += rectHeight;
                                }
                            }
                        }


                        for (int j = 0; j < (i + 1) && isSthBlocking; j++) //JUST CHANGED to +1
                        {
                            isSthBlocking = false;

                            currentEndPoint = new Vector3(0, -10000, 0);

                            if (useoldLODs)
                            {

                                if (showLOD)
                                {

                                    if (attractions[i].goalLoD == (int)Attraction.LoD.MID || attractions[i].goalLoD == (int)Attraction.LoD.HIGH)
                                    {
                                        attractions[i].WaitImage.rectTransform.GetWorldCorners(currentCorners);
                                    }
                                    else if (attractions[i].goalLoD == (int)Attraction.LoD.LOW)
                                    {
                                        attractions[i].CubeImage.rectTransform.GetWorldCorners(currentCorners);
                                    }
                                }
                                else
                                {
                                    attractions[i].WaitImage.rectTransform.GetWorldCorners(currentCorners);
                                }
                            } else
                            {
                                if (showLOD && updateLoD)  //JUST CHANGED
                                {
                                    if (currentLODState == 0)
                                    {
                                        attractions[i].CubeImage.rectTransform.GetWorldCorners(currentCorners);
                                        attractions[i].goalLoD = (int)Attraction.LoD.LOW;

                                        attractions[i].SmallCubeTransform.gameObject.layer = 9;
                                        attractions[i].CubeTransform.gameObject.layer = 0;
                                    } else if (currentLODState == 1)
                                    {
                                        attractions[i].goalLoD = (int)Attraction.LoD.MID;
                                        attractions[i].WaitImage.rectTransform.GetWorldCorners(currentCorners);

                                        attractions[i].SmallCubeTransform.gameObject.layer = 0;
                                        attractions[i].CubeTransform.gameObject.layer = 9;
                                    } else
                                    {
                                        attractions[i].goalLoD = (int)Attraction.LoD.HIGH;
                                        attractions[i].WaitImage.rectTransform.GetWorldCorners(currentCorners);

                                        attractions[i].SmallCubeTransform.gameObject.layer = 0;
                                        attractions[i].CubeTransform.gameObject.layer = 9;
                                    }

                                    /*if (attractions[i].goalLoD == (int)Attraction.LoD.MID || attractions[i].goalLoD == (int)Attraction.LoD.HIGH)
                                    {
                                        attractions[i].WaitImage.rectTransform.GetWorldCorners(currentCorners);
                                    }
                                    else if (attractions[i].goalLoD == (int)Attraction.LoD.LOW)
                                    {
                                        attractions[i].CubeImage.rectTransform.GetWorldCorners(currentCorners);
                                    }*/
                                }
                                else
                                {
                                    attractions[i].WaitImage.rectTransform.GetWorldCorners(currentCorners); // DANGEROUS PLACE

                                    if (showLOD)
                                    {
                                        if (attractions[i].currentLoD == (int)Attraction.LoD.LOW)
                                        {
                                            attractions[i].CubeImage.rectTransform.GetWorldCorners(currentCorners);
                                        }
                                    } /*else
                                    {
                                        
                                    }*/
                                }
                            }

                            if (showLowestLOD || showOnlyText)
                            {
                                attractions[i].CubeImage.rectTransform.GetWorldCorners(currentCorners);
                            }
                            

                            currentLabelPosition = attractions[i].WaitImage.rectTransform.position;

                            currentNormalVector = Vector3.Cross(currentCorners[2] - currentCorners[1], currentCorners[0] - currentCorners[1]);

                            vectorOne = new Vector2(attractions[i].WaitImage.transform.position.x, attractions[i].WaitImage.transform.position.z);
                            vectorTwo = new Vector2(attractions[j].WaitImage.transform.position.x, attractions[j].WaitImage.transform.position.z);

                            compareAngle = Vector2.Angle(vectorOne, vectorTwo);

                            //if (compareAngle <= 90)
                            //{

                            if (Physics.Raycast(Camera.main.transform.position, (currentCorners[0] - Camera.main.transform.position), out hitt1, (currentCorners[0] - Camera.main.transform.position).magnitude - 10, 9))
                            {
                                directionTop = hitt1.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);

                                if (hitt1.collider.gameObject.name.Contains("Large"))
                                {
                                    directionTop = hitt1.collider.gameObject.transform.position + new Vector3(0, attractions[0].CubeImage.rectTransform.sizeDelta.y / 2);
                                }

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

                                    if (attractions[i].Name == "Splash Mountain")
                                    {
                                        //debugTextObject.GetComponent<Text>().text += hitt1.collider.name + "1|";// + hitt1.collider.gameObject.transform.position;
                                    }
                                    outString += hitt1.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);//currentEndPoint.y + hitt1.collider.name + " ";
                                }
                            }

                            if (Physics.Raycast(Camera.main.transform.position, (currentCorners[1] - Camera.main.transform.position), out hitt2, (currentCorners[1] - Camera.main.transform.position).magnitude - 10, 9))
                            {
                                directionTop = hitt2.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);

                                if (hitt2.collider.gameObject.name.Contains("Large"))
                                {
                                    directionTop = hitt2.collider.gameObject.transform.position + new Vector3(0, attractions[0].CubeImage.rectTransform.sizeDelta.y / 2);
                                }

                                directionTop = (directionTop - Camera.main.transform.position).normalized;

                                hitPlane = new Plane(currentNormalVector, currentLabelPosition);


                                collisionRay = new Ray(Camera.main.transform.position, directionTop);
                                hitPlane.Raycast(collisionRay, out distValue);
                                endPoint = collisionRay.GetPoint(distValue);

                                if (endPoint.y > currentEndPoint.y)
                                {
                                    currentEndPoint = endPoint;
                                    currentHit = hitt2;
                                    isSthBlocking = true;

                                    if (attractions[i].Name == "Splash Mountain")
                                    {
                                        //debugTextObject.GetComponent<Text>().text += hitt2.collider.name + "2|";// + "|" + hitt2.collider.gameObject.transform.position;
                                    }
                                    outString += hitt2.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);
                                }
                            }

                            if (Physics.Raycast(Camera.main.transform.position, (currentCorners[2] - Camera.main.transform.position), out hitt3, (currentCorners[2] - Camera.main.transform.position).magnitude - 10, 9))
                            {
                                directionTop = hitt3.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);

                                if (hitt3.collider.gameObject.name.Contains("Large"))
                                {
                                    directionTop = hitt3.collider.gameObject.transform.position + new Vector3(0, attractions[0].CubeImage.rectTransform.sizeDelta.y / 2);
                                }

                                directionTop = (directionTop - Camera.main.transform.position).normalized;

                                hitPlane = new Plane(currentNormalVector, currentLabelPosition);

                                collisionRay = new Ray(Camera.main.transform.position, directionTop);
                                hitPlane.Raycast(collisionRay, out distValue);
                                endPoint = collisionRay.GetPoint(distValue);

                                if (endPoint.y > currentEndPoint.y)
                                {
                                    currentEndPoint = endPoint;
                                    currentHit = hitt3;
                                    isSthBlocking = true;

                                    if (attractions[i].Name == "Splash Mountain")
                                    {
                                        //debugTextObject.GetComponent<Text>().text += hitt3.collider.name + "3|";// + "|" + hitt3.collider.gameObject.transform.position;
                                    }
                                    outString += hitt3.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);
                                }

                            }

                            if (Physics.Raycast(Camera.main.transform.position, (currentCorners[3] - Camera.main.transform.position), out hitt4, (currentCorners[3] - Camera.main.transform.position).magnitude - 10, 9))
                            {
                                directionTop = hitt4.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);

                                if (hitt4.collider.gameObject.name.Contains("Large"))
                                {
                                    directionTop = hitt4.collider.gameObject.transform.position + new Vector3(0, attractions[0].CubeImage.rectTransform.sizeDelta.y / 2);
                                }

                                directionTop = (directionTop - Camera.main.transform.position).normalized;

                                hitPlane = new Plane(currentNormalVector, currentLabelPosition);

                                collisionRay = new Ray(Camera.main.transform.position, directionTop);
                                hitPlane.Raycast(collisionRay, out distValue);
                                endPoint = collisionRay.GetPoint(distValue);

                                if (endPoint.y > currentEndPoint.y)
                                {
                                    currentEndPoint = endPoint;
                                    currentHit = hitt4;
                                    isSthBlocking = true;


                                    if (attractions[i].Name == "Splash Mountain")
                                    {
                                        //debugTextObject.GetComponent<Text>().text += hitt4.collider.name + "4|";// + "|" + hitt4.collider.gameObject.transform.position;
                                        //debugTextObject.GetComponent<Text>().text += hitt4.collider.transform.position + "--" + currentCorners[3]
                                    }
                                    outString += hitt4.collider.gameObject.transform.position + new Vector3(0, attractions[0].WaitImage.rectTransform.sizeDelta.y / 2);
                                }

                            }


                            if (isSthBlocking)
                            {

                                //if (currentEndPoint.y > attractions[i].AttractionBackground.transform.position.y)
                                //{
                                /*attractions[i].AttractionBackground.transform.position =
                                    new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                    currentEndPoint.y + 20/*(attractions[i].AttractionBackground.rectTransform.sizeDelta.y / 2),*/
                                //attractions[i].AttractionBackground.transform.position.z);


                                if (showLOD)
                                {
                                    if (attractions[i].goalLoD == (int)Attraction.LoD.MID || attractions[i].goalLoD == (int)Attraction.LoD.HIGH)
                                    {
                                        attractions[i].shiftTo(new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                        currentEndPoint.y - 25 + 5, //5 margin
                                        attractions[i].AttractionBackground.transform.position.z));
                                    }
                                    else if (attractions[i].goalLoD == (int)Attraction.LoD.LOW)
                                    {
                                        attractions[i].shiftTo(new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                        currentEndPoint.y + 25 + 5, //5 margin
                                        attractions[i].AttractionBackground.transform.position.z));
                                    }
                                } else
                                {
                                    if (showLowestLOD)
                                    {
                                        attractions[i].shiftTo(new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                        currentEndPoint.y + 25 + 5, //5 margin
                                        attractions[i].AttractionBackground.transform.position.z));
                                    }
                                    else if (showOnlyText)
                                    {
                                        //attractions[i].shiftTo(new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                        //currentEndPoint.y - 20, //5 margin
                                        //attractions[i].AttractionBackground.transform.position.z));

                                        attractions[i].shiftTo(new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                        currentEndPoint.y - 20, //5 margin
                                        attractions[i].AttractionBackground.transform.position.z));
                                    }
                                    else
                                    {

                                        attractions[i].shiftTo(new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                           currentEndPoint.y - 25 + 5, //5 margin
                                           attractions[i].AttractionBackground.transform.position.z));
                                    }


                                }

                                //attractions[i].SmallCubeTransform.position = new Vector3(attractions[i].AttractionBackground.transform.position.x,
                                //currentEndPoint.y + 60 /*(attractions[i].AttractionBackground.rectTransform.sizeDelta.y / 2)*/,
                                // attractions[i].AttractionBackground.transform.position.z);

                                //attractions[i].AttractionBackground.transform.LookAt(new Vector3(0, attractions[i].AttractionBackground.transform.position.y, 0));
                                //attractions[i].AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                                attractions[i].AttractionBackground.transform.LookAt(new Vector3(0, attractions[i].AttractionBackground.transform.position.y, 0));
                                attractions[i].AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);
                                //}
                                //Canvas.ForceUpdateCanvases();

                                //yield return new WaitForFixedUpdate();
                            }


                            //outString += attractions[0].Name + "|||";
                            //}
                        }

                        if (!useoldLODs && updateLoD && showLOD)
                        {
                            Vector3 straightLine = new Vector3(attractions[i].AttractionBackground.transform.position.x, 0, attractions[i].AttractionBackground.transform.position.z);
                            Vector3 currentVector = attractions[i].AttractionBackground.transform.position + new Vector3(0, 60, 0);
                            float upwardsAngle = Vector3.Angle(straightLine, currentVector);

                            //debugTextObject.GetComponent<Text>().text += "a" + upwardsAngle + " ";// attractions[0].AttractionBackground.gameObject.transform.GetChild(4) + "";// cube.name + " XXX";

                            /* OLD START
                            if (upwardsAngle > 20 && upwardsAngle < 30)
                            {
                                currentLODState = 1;
                                
                            } else if (upwardsAngle >= 30)
                            {
                                currentLODState = 2;
                            }

                            if (i > (attractions.Count * 0.66f))
                            {
                                currentLODState = 2;
                            } OLD END*/

                            if (currentLODState == 0 && upwardsAngle > 20 && upwardsAngle < 30)
                            {
                                currentLODState = 1;

                            }
                            else if ((currentLODState == 0 || currentLODState == 1) && upwardsAngle >= 30)
                            {
                                currentLODState = 2;
                            }
                            else if (currentLODState == 0 && i > (attractions.Count * 0.33f) && i <= (attractions.Count * 0.66f))
                            {
                                currentLODState = 1;
                            }
                            else if (currentLODState == 1 && i > (attractions.Count * 0.66f))
                            {
                                currentLODState = 2;
                            }
                        }




                        //debugTextObject.GetComponent<Text>().text = outString;

                        //if (attractions[i].Name == "Beaver Brothers Explorer Canoes")
                        //{
                        //debugTextObject.GetComponent<Text>().text = outString;// currentEndPoint.y + "";
                        //}
                        //debugTextObject.GetComponent<Text>().text = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta + " " + attractions[i].AttractionBackground.gameObject.transform.GetChild(4).gameObject.GetComponent<RectTransform>().sizeDelta + "";


                        //attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta = attractions[i].AttractionBackground.gameObject.transform.GetChild(4).gameObject.GetComponent<RectTransform>().sizeDelta;
                        //attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].transform.position = attractions[i].AttractionBackground.gameObject.transform.GetChild(4).position;

                        //attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta = attractions[i].AttractionBackground.GetComponentsInChildren<PrimitiveType>()[0].rectTransform

                        /*attractions[i].goalPosition = attractions[i].AttractionBackground.transform.position;
                        attractions[i].speed = (attractions[i].AttractionBackground.transform.position - initialPosition.position) / 100.0f;
                        attractions[i].AttractionBackground.transform.position = initialPosition.position;*/


                        //attractions[i].AttractionBackground.transform.LookAt(new Vector3(0, 0, 0));
                        //attractions[i].AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);


                        //attractions[i].Distance = attractions[i].AttractionBackground.transform.position.magnitude;
                    }


                    // debugTextObject.GetComponent<Text>().text += occupiedHeight + " " + (Screen.height / 2);

                    for (int i = 0; i < attractions.Count; i++)
                    {
                        attractions[i].goalPosition = attractions[i].AttractionBackground.transform.position;
                        attractions[i].speed = (attractions[i].AttractionBackground.transform.position - attractions[i].initialPosition) / 100.0f;
                        //attractions[i].AttractionBackground.transform.position = attractions[i].initialPosition;

                        attractions[i].shiftTo(attractions[i].initialPosition);

                        if (attractions[i].Name == "Pirates of the Caribbean")
                        {
                            //debugTextObject.GetComponent<Text>().text += System.Environment.NewLine + attractions[i].goalPosition;
                        }
                    }


                    //yield return new WaitForFixedUpdate();
                    
                    if (showAreas)
                    {
                        for (int i = 0; i < areas.Count; i++)
                        {
                            if (areas[i].initTransition)
                            {
                                areas[i].prepareForTransition();
                            }
                        }
                    }

                    int afterTime = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                    //debugTextObject.GetComponent<Text>().text += System.Environment.NewLine + (afterTime - beforeTime) + "";
                    /*}
                    catch (Exception ex)
                    {
                        debugTextObject.GetComponent<Text>().text = ex.ToString() + " BBB";
                    }*/


                    //Canvas.ForceUpdateCanvases();

                    //debugTextObject.GetComponent<Text>().text = Camera.main.transform.position + "";
                    //debugTextObject.GetComponent<Text>().text = "";// + attractions[0].AttractionBackground.gameObject.transform.GetChild(4).transform.position + " " + attractions[0].AttractionBackground.transform.position;

                    shiftReady = true;

                    distanceMoved = false;

                    occlusionHandlingInProgress = false;


                }
                catch (Exception ex)
                {
                    debugTextObject.GetComponent<Text>().text += ex.ToString() + " Occlusion Handling";
                }


                yield return new WaitForSeconds(2.5f);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
    #endregion
    
    #region AugmentedScript ShiftObjects
    IEnumerator ShiftObjects()
    {
        bool running = true;

        numberOfShifts = 0;

        while (running)
        {
            try { 
            //for (int i = 0; i < attractions.Count; i++)
            //{
                /*if (attractions[i].Name == "Peter Pan’s Flight")
                {
                    debugTextObject.GetComponent<Text>().text = "";

                    if (attractions[i].goalPosition != null)
                    {
                        debugTextObject.GetComponent<Text>().text = attractions[i].goalPosition + " i:" + attractions[i].initialPosition + System.Environment.NewLine;
                    }

                    debugTextObject.GetComponent<Text>().text += " p:" + attractions[i].transform.position;
                }*/
            //}

            if (shiftReady && numberOfShifts <= 100 && !occlusionHandlingInProgress)
            {

                //debugTextObject.GetComponent<Text>().text = "" + numberOfShifts;

                for (int i = 0; i < attractions.Count; i++)
                {
                        //attractions[i].AttractionBackground.transform.position += attractions[i].speed;


                        if (numberOfShifts == 100)
                        {
                            //debugTextObject.GetComponent<Text>().text += " 100";
                            if (showLOD && !showAreas)
                            {
                                attractions[i].currentLoD = attractions[i].goalLoD;
                                attractions[i].lodChangeCompletion = 0.0f;
                            }
                            /*if (attractions[i].currentLoD == (int)Attraction.LoD.LOW)
                            {
                                
                            }*/

                        }
                        else
                        {
                            //attractions[i].move(attractions[i].speed, false);
                            attractions[i].moveEase(numberOfShifts / 100.0f, EasingFunction.Ease.EaseInOutSine);


                            if (showLOD && !showAreas)
                            {
                                if (attractions[i].currentLoD != attractions[i].goalLoD)
                                {
                                    attractions[i].changeTextTransparency(debugTextObject);
                                }
                            }                        
                        }


                    //= attractions[i].goalPosition; //+= attractions[i].speed;

                    /*if (i == 0)
                    {
                        debugTextObject.GetComponent<Text>().text = attractions[0].goalPosition - attractions[0].AttractionBackground.transform.position + System.Environment.NewLine +
                            attractions[0].speed + attractions[0].Name;
                    }*/
                    //}
                    //shiftReady = false;

                    /*if (Mathf.Abs(attractions[i].goalPosition.x - attractions[i].AttractionBackground.transform.position.x) <= 10 &&
                    Mathf.Abs(attractions[i].goalPosition.y - attractions[i].AttractionBackground.transform.position.y) <= 10 &&
                    Mathf.Abs(attractions[i].goalPosition.z - attractions[i].AttractionBackground.transform.position.z) <= 10)
                    {                        
                        attractions[i].AttractionBackground.transform.position = attractions[i].goalPosition;
                    }*/

                    //EditorUtility.SetDirty(attractions[i].AttractionBackground);

                    /*if (attractions[i].Name == "Peter Pan’s Flight")
                    {
                        debugTextObject.GetComponent<Text>().text = attractions[i].AttractionBackground.transform.position + System.Environment.NewLine;
                        debugTextObject.GetComponent<Text>().text += attractions[i].AttractionBackground.rectTransform.position + System.Environment.NewLine;
                        debugTextObject.GetComponent<Text>().text += "AI " + attractions[i].AttractionImage.transform.position + System.Environment.NewLine;
                        debugTextObject.GetComponent<Text>().text += "WI " + attractions[i].WaitImage.transform.position + System.Environment.NewLine;
                    }*/


                    /*debugTextObject.GetComponent<Text>().text = "";
                    foreach (Area a in areas)
                    {
                        debugTextObject.GetComponent<Text>().text += a.Name + "|";
                    }*/

                }

                //debugTextObject.GetComponent<Text>().text = areas[0].SubAttractions[0].WaitImage.transform.position + " " + System.Environment.NewLine + areas[0].SubAttractions[0].SmallCubeTransform.position;

                numberOfShifts++;

                if (showAreas)
                {
                    bool firstInRound = true;

                    foreach (Area currentArea in areas)
                    {
                            if (currentArea.initTransition)
                            {
                                currentArea.changeTransparency();
                            }
                            else
                            {
                                foreach (Attraction subAttraction in currentArea.SubAttractions)
                                {
                                    if (numberOfShifts == 100)
                                    {
                                        subAttraction.currentLoD = subAttraction.goalLoD;
                                        subAttraction.lodChangeCompletion = 0.0f;
                                    }
                                    else
                                    {
                                        if (subAttraction.currentLoD != subAttraction.goalLoD)
                                        {
                                            subAttraction.changeTextTransparency(debugTextObject);
                                        }
                                    }
                                }
                            }

                            /*if (firstRegionChange && screenshots && firstInRound)
                            {
                                if (((numberOfShifts - 1) % 10) == 0)
                                {
                                    string myFileName = System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + "_" + numberOfShifts + ".png";
                                    string myDefaultLocation = Application.persistentDataPath + "/AR/" + myFileName;
                                    //string myFolderLocation = "/storage/emulated/0/DCIM/Camera/JCB/";  //EXAMPLE OF DIRECTLY ACCESSING A CUSTOM FOLDER OF THE GALLERY
                                    string myScreenshotLocation = myDefaultLocation + myFileName;

                                    //ENSURE THAT FOLDER LOCATION EXISTS
                                    if (!System.IO.Directory.Exists(myDefaultLocation))
                                    {
                                        System.IO.Directory.CreateDirectory(myDefaultLocation);
                                    }

                                    ScreenCapture.CaptureScreenshot(myFileName);
                                }
                            }*/

                    }

                    firstInRound = false;
                }


                if (numberOfShifts == 101)
                {
                    shiftReady = false;
                    numberOfShifts = 0;

                    if (showAreas)
                    {
                        foreach (Area currentArea in areas)
                        {
                            if (currentArea.initTransition)
                            {
                                if (!currentArea.isExpanded)
                                {
                                    currentArea.moveLabelsToGoal();
                                }

                                firstRegionChange = false;
                            }

                            currentArea.initTransition = false;
                            //currentArea.transparency = 0.0f;
                        }
                    }
                }
                //debugTextObject.GetComponent<Text>().text = attractions[0].AttractionBackground.transform.position + " " + attractions[0].AttractionBackground.gameObject.transform.GetChild(4).position;
            }

            Canvas.ForceUpdateCanvases();

            }
            catch (Exception ex)
            {
                debugTextObject.GetComponent<Text>().text = ex.ToString() + " Shift";
            }

            yield return new WaitForSeconds(0.015f);
        }
    }
    #endregion

    #region AugmentedScript MoveLabels
    IEnumerator MoveLabels()
    {
        bool running = true;
               

        while (running)
        {
            try { 
             
            Rect rectOne;
            Rect rectTwo;

            //visibleAttractions = attractions;

            float rectHeight;
            float occupiedHeight = 0;

            float heightLoD1 = 0.0f;
            float heightLoD2 = 0.0f;

            int numLoD1 = 0;
            int numLoD2 = 0;

            float forwardFactor = -1.0f;

                bool leftRight = false;

                if ((reverseButtonDown || forwardButtonDown) && !occlusionHandlingInProgress)
                {
                    firstRegionChange = true;

                    if (reverseButtonDown)
                    {
                        forwardFactor *= -1;
                    }

                    int mS = 2;
                    for (int i = 0; i < attractions.Count; i++)
                    {
                        if (true)
                        {
                            if (leftRight)
                            {
                                attractions[i].move(new Vector3(Camera.main.transform.forward.z, 0, Camera.main.transform.forward.x * -1) * forwardFactor * /*4*/mS, true);
                                attractions[i].initialPosition += new Vector3(Camera.main.transform.forward.z, 0, Camera.main.transform.forward.x * -1) * forwardFactor * /*4*/mS;
                                attractions[i].goalPosition += new Vector3(Camera.main.transform.forward.z, 0, Camera.main.transform.forward.x * -1) * forwardFactor * /*4*/mS;

                            }
                            else
                            {
                                attractions[i].move(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * forwardFactor * /*4*/mS, true);
                                attractions[i].initialPosition += new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * forwardFactor * /*4*/mS;
                                attractions[i].goalPosition += new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * forwardFactor * /*4*/mS;
                            }
                        }
                    }

                    for (int i = 0; i < areas.Count; i++)
                    {
                        if (true)
                        {
                            if (leftRight)
                            {
                                areas[i].move(new Vector3(Camera.main.transform.forward.z, 0, Camera.main.transform.forward.x * -1) * forwardFactor * mS);
                            }
                            else
                            {
                                areas[i].move(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * forwardFactor * mS);
                            }
                        }
                    }

                    currentPosition += new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z) * forwardFactor * mS;// 4;
                }    

            Canvas.ForceUpdateCanvases();
            //            attractions.Sort();
            //    visibleAttractions.Clear();

            //    try
            //    {
            //        for (int i = 0; i < attractions.Count; i++)
            //        {
            //            if (attractions[i].isVisible())
            //            {
            //                rectHeight = attractions[i].GUIRectWithObject();
            //                occupiedHeight += rectHeight;
            //                visibleAttractions.Add(i);
            //            }
            //        }

            //        //debugTextObject.GetComponent<Text>().text = occupiedHeight + " " + Screen.height;

            //        heightLoD1 = 0.7f * (Screen.height - occupiedHeight);
            //        heightLoD2 = 0.3f * (Screen.height - occupiedHeight);

            //        //debugTextObject.GetComponent<Text>().text = "H1: " + heightLoD1 + " H2: " + heightLoD2;
            //        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //        // debugTextObject.GetComponent<Text>().text = attractions[visibleAttractions[0]].Name + " H1: " + attractions[visibleAttractions[0]].GUIRectWithObject(1) + " H2: " + attractions[0].GUIRectWithObject();
            //        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //    }
            //    catch (Exception ex)
            //    {
            //        debugTextObject.GetComponent<Text>().text = ex.Message;
            //    }


            //    bool isClosestVisible = true;

            //    Rect rect;
            //    RectTransformData currentRectTransform;


            //    RaycastHit hitt1, hitt2, hitt3, hitt4;

            //    Vector3[] currentCorners = new Vector3[4];

            //    float maxHeightShift = 0.0f;
            //    Vector3 directionTop = new Vector3(), directionBottom, endPoint = new Vector3();
            //    Vector3 currentLabelPosition;
            //    Vector3 currentNormalVector;

            //    Plane hitPlane;
            //    float distValue;
            //    Ray collisionRay;

            //    Vector3 currentEndPoint = new Vector3(0, -10000, 0);
            //    RaycastHit currentHit = new RaycastHit();

            //    string howManyHits = "";


            //    string outMessage = "", aaa = "";
            //    bool isSthBlocking = true;

            //    for (int i = 0; i < attractions.Count; i++)
            //    {
            //        if (attractions[i].AttractionBackground.GetComponentsInChildren<Component>()[12].GetComponent<MeshRenderer>().isVisible)
            //        {
            //            if (!isClosestVisible)
            //            {
            //                isSthBlocking = false;

            //                currentEndPoint = new Vector3(0, -10000, 0);

            //                attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.GetWorldCorners(currentCorners);
            //                currentLabelPosition = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.position;
            //                currentNormalVector = Vector3.Cross(currentCorners[2] - currentCorners[1], currentCorners[0] - currentCorners[1]);

            //                if (Physics.Raycast(Camera.main.transform.position, (currentCorners[0] - Camera.main.transform.position), out hitt1, (currentCorners[0] - Camera.main.transform.position).magnitude - 10, 9))
            //                {
            //                    isSthBlocking = true;
            //                    /*directionTop = hitt1.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
            //                    directionTop = (directionTop - Camera.main.transform.position).normalized;

            //                    hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);

            //                    collisionRay = new Ray(Camera.main.transform.position, directionTop);
            //                    hitPlane.Raycast(collisionRay, out distValue);
            //                    endPoint = collisionRay.GetPoint(distValue);

            //                    if (endPoint.y > currentEndPoint.y)
            //                    {
            //                        currentEndPoint = endPoint;
            //                        currentHit = hitt1;
            //                        isSthBlocking = true;
            //                    }*/
            //                }

            //                if (Physics.Raycast(Camera.main.transform.position, (currentCorners[1] - Camera.main.transform.position), out hitt2, (currentCorners[1] - Camera.main.transform.position).magnitude - 10, 9))
            //                {
            //                    isSthBlocking = true;
            //                    /*directionTop = hitt2.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
            //                    directionTop = (directionTop - Camera.main.transform.position).normalized;

            //                    hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);


            //                    collisionRay = new Ray(Camera.main.transform.position, directionTop);
            //                    hitPlane.Raycast(collisionRay, out distValue);
            //                    endPoint = collisionRay.GetPoint(distValue);

            //                    if (endPoint.y > currentEndPoint.y)
            //                    {
            //                        currentEndPoint = endPoint;
            //                        currentHit = hitt2;
            //                        isSthBlocking = true;
            //                    }*/
            //                }

            //                if (Physics.Raycast(Camera.main.transform.position, (currentCorners[2] - Camera.main.transform.position), out hitt3, (currentCorners[2] - Camera.main.transform.position).magnitude - 10, 9))
            //                {
            //                    isSthBlocking = true;
            //                    /*directionTop = hitt3.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
            //                    directionTop = (directionTop - Camera.main.transform.position).normalized;

            //                    hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);

            //                    collisionRay = new Ray(Camera.main.transform.position, directionTop);
            //                    hitPlane.Raycast(collisionRay, out distValue);
            //                    endPoint = collisionRay.GetPoint(distValue);

            //                    if (endPoint.y > currentEndPoint.y)
            //                    {
            //                        currentEndPoint = endPoint;
            //                        currentHit = hitt3;
            //                        isSthBlocking = true;
            //                    }*/

            //                }

            //                if (Physics.Raycast(Camera.main.transform.position, (currentCorners[3] - Camera.main.transform.position), out hitt4, (currentCorners[3] - Camera.main.transform.position).magnitude - 10, 9))
            //                {
            //                    isSthBlocking = true;
            //                    /*directionTop = hitt4.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
            //                    directionTop = (directionTop - Camera.main.transform.position).normalized;

            //                    hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);

            //                    collisionRay = new Ray(Camera.main.transform.position, directionTop);
            //                    hitPlane.Raycast(collisionRay, out distValue);
            //                    endPoint = collisionRay.GetPoint(distValue);

            //                    if (endPoint.y > currentEndPoint.y)
            //                    {
            //                        currentEndPoint = endPoint;
            //                        currentHit = hitt4;
            //                        isSthBlocking = true;
            //                    }*/

            //                }


            //                if (isSthBlocking)
            //                {
            //                    attractions[i].AttractionBackground.transform.position += new Vector3(0, 1, 0);
            //                    /*attractions[i].AttractionBackground.transform.position =
            //                        new Vector3(attractions[i].AttractionBackground.transform.position.x,
            //                        currentEndPoint.y + (attractions[i].AttractionBackground.rectTransform.sizeDelta.y / 2),
            //                        attractions[i].AttractionBackground.transform.position.z);     */                       
            //                }

            //            } else
            //            {
            //                isClosestVisible = false;
            //            }
            //        }
            //    }


            //        for (int i = 0; i < attractions.Count; i++)
            //    {
            //        //debugTextObject.GetComponent<Text>().text = "" + attractions[i].AttractionBackground.GetComponentsInChildren<Component>().Length;


            //        if (true/*attractions[i].AttractionBackground.GetComponentsInChildren<Component>()[12].GetComponent<MeshRenderer>().isVisible*/)
            //        {

            //            if (forwardButtonDown /*&& attractions[i].AttractionBackground.transform.position.magnitude > 400*/)
            //            {
            //                //Camera.main.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 10);

            //                //attractions[i].AttractionBackground.transform.position += (Camera.main.transform.InverseTransformDirection * 4);
            //                //attractions[j].AttractionBackground.transform.position -= attractions[j].AttractionBackground.transform.position.normalized;

            //                //attractions[i].AttractionBackground.transform.position -= attractions[i].AttractionBackground.transform.position.normalized;
            //                //attractions[j].AttractionBackground.transform.position -= attractions[j].AttractionBackground.transform.position.normalized;


            //                attractions[i].AttractionBackground.transform.position -= new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * 12; //new Vector3(0, 0, 12);
            //            }


            //            attractions[i].currentLoD = 2;
            //            numLoD2++;

            //            var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //            textTransColor.a = 0;
            //            attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //            var backColor = attractions[i].AttractionBackground.color;
            //            backColor.a = 0;
            //            attractions[i].AttractionBackground.color = backColor;

            //            var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //            texTransColor.a = 1;
            //            attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;
            //            /*if (visibleAttractions.Contains(i))
            //            {
            //                if (heightLoD1 > 0)
            //                {
            //                    heightLoD1 -= attractions[i].GUIRectWithObject(1);
            //                    attractions[i].currentLoD = 1;

            //                    numLoD1++;

            //                    var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //                    textTransColor.a = 1;
            //                    attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //                    var backColor = attractions[i].AttractionBackground.color;
            //                    backColor.a = 1;
            //                    attractions[i].AttractionBackground.color = backColor;

            //                    var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                    texTransColor.a = 1;
            //                    attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;

            //                } else if (heightLoD2 > 0)
            //                {
            //                    heightLoD2 -= attractions[i].GUIRectWithObject(2);
            //                    attractions[i].currentLoD = 2;
            //                    numLoD2++;

            //                    var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //                    textTransColor.a = 0;
            //                    attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //                    var backColor = attractions[i].AttractionBackground.color;
            //                    backColor.a = 0;
            //                    attractions[i].AttractionBackground.color = backColor;

            //                    var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                    texTransColor.a = 1;
            //                    attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;

            //                } else
            //                {
            //                    attractions[i].currentLoD = 3;

            //                    var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //                    textTransColor.a = 0;
            //                    attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //                    var backColor = attractions[i].AttractionBackground.color;
            //                    backColor.a = 0;
            //                    attractions[i].AttractionBackground.color = backColor;

            //                    var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                    texTransColor.a = 0;
            //                    attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;
            //                }
            //            }*/


            //            ///

            //            for (int j = 0; j < attractions.Count; j++)
            //            {

            //                if (attractions[i].AttractionBackground.GetComponentsInChildren<Component>()[12].GetComponent<MeshRenderer>().isVisible)
            //                {

            //                    /*if (attractions[i].AttractionBackground.transform.position.magnitude >= 1000 && attractions[i].AttractionBackground.transform.position.magnitude <= 1200)
            //                    {
            //                        var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                        texTransColor.a = (1 - ((attractions[i].AttractionBackground.transform.position.magnitude - 1000) / 200));
            //                        attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;
            //                    }

            //                    if (attractions[i].AttractionBackground.transform.position.magnitude >= 600 && attractions[i].AttractionBackground.transform.position.magnitude <= 800)
            //                    {
            //                        var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //                        textTransColor.a = (1 - ((attractions[i].AttractionBackground.transform.position.magnitude - 600) / 200));
            //                        attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //                        var backColor = attractions[i].AttractionBackground.color;
            //                        backColor.a = (1 - ((attractions[i].AttractionBackground.transform.position.magnitude - 600) / 200));
            //                        attractions[i].AttractionBackground.color = backColor;
            //                    }


            //                    if (attractions[i].AttractionBackground.transform.position.magnitude < 1000)
            //                    {
            //                        var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                        texTransColor.a = 1;
            //                        attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;
            //                    }

            //                        if (attractions[i].AttractionBackground.transform.position.magnitude < 600)
            //                    {
            //                        var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //                        textTransColor.a = 1;
            //                        attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //                        var backColor = attractions[i].AttractionBackground.color;
            //                        backColor.a = 1;
            //                        attractions[i].AttractionBackground.color = backColor;

            //                        var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                        texTransColor.a = 1;
            //                        attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;
            //                    }

            //                    if (attractions[i].AttractionBackground.transform.position.magnitude > 1200)
            //                    {
            //                        var textTransColor = attractions[i].AttractionBackground.GetComponentInChildren<Text>().color;
            //                        textTransColor.a = 0;
            //                        attractions[i].AttractionBackground.GetComponentInChildren<Text>().color = textTransColor;

            //                        var backColor = attractions[i].AttractionBackground.color;
            //                        backColor.a = 0;
            //                        attractions[i].AttractionBackground.color = backColor;

            //                        var texTransColor = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color;
            //                        texTransColor.a = 0;
            //                        attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[1].color = texTransColor;
            //                    }*/


            //                    if (i != j)
            //                    {
            //                        rectOne = new Rect(attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].transform.position.x - (attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.x / 2),
            //                            attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].transform.position.y - (attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y),
            //                            float.Parse(Math.Round(attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.x).ToString()), float.Parse(Math.Round(attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y).ToString()));

            //                        rectTwo = new Rect(attractions[j].AttractionBackground.GetComponentsInChildren<RawImage>()[0].transform.position.x - (attractions[j].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.x / 2),
            //                            attractions[j].AttractionBackground.GetComponentsInChildren<RawImage>()[0].transform.position.y - (attractions[j].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y),
            //                            float.Parse(Math.Round(attractions[j].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.x).ToString()), float.Parse(Math.Round(attractions[j].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y).ToString()));

            //                        if (attractions[i].AttractionBackground.transform.position.magnitude <= 800)
            //                        {
            //                            rectOne = new Rect(attractions[i].AttractionBackground.transform.position.x,
            //                            attractions[i].AttractionBackground.transform.position.y,
            //                            float.Parse(Math.Round(attractions[i].AttractionBackground.rectTransform.sizeDelta.x).ToString()), float.Parse(Math.Round(attractions[i].AttractionBackground.rectTransform.sizeDelta.y * 4).ToString()));

            //                            rectTwo = new Rect(attractions[j].AttractionBackground.transform.position.x,
            //                            attractions[j].AttractionBackground.transform.position.y,
            //                            float.Parse(Math.Round(attractions[j].AttractionBackground.rectTransform.sizeDelta.x).ToString()), float.Parse(Math.Round(attractions[j].AttractionBackground.rectTransform.sizeDelta.y * 4).ToString()));
            //                        }

            //                        if (rectOne.Overlaps(rectTwo))
            //                        {
            //                            /*if (rectOne.y < rectTwo.y)
            //                            {
            //                                attractions[i].AttractionBackground.transform.position -= new Vector3(0, 1, 0);
            //                                attractions[j].AttractionBackground.transform.position += new Vector3(0, 1, 0);
            //                            }
            //                            else
            //                            {
            //                                attractions[i].AttractionBackground.transform.position += new Vector3(0, 1, 0);
            //                                attractions[j].AttractionBackground.transform.position -= new Vector3(0, 1, 0);
            //                            }*/
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //        //attractions[i].AttractionBackground.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
            //        attractions[i].AttractionBackground.transform.LookAt(new Vector3(0, 0, 0));

            //        /*attractions[i].AttractionBackground.transform.rotation = Quaternion.Euler(attractions[i].AttractionBackground.transform.rotation.x,
            //            attractions[i].AttractionBackground.transform.rotation.y + 0,
            //            attractions[i].AttractionBackground.transform.rotation.z);*/
            //        attractions[i].AttractionBackground.transform.Rotate(new Vector3(0, 1, 0), 180.0f);


            //        attractions[i].Distance = attractions[i].AttractionBackground.transform.position.magnitude;



            //        //********************************************************************************************************************************************************************************************************
            //        //debugTextObject.GetComponent<Text>().text = attractions[visibleAttractions[0]].Name + " H1: " + attractions[visibleAttractions[0]].GUIRectWithObject(1) + " H2: " + attractions[0].GUIRectWithObject()
            //        //    + " N1 " + numLoD1 +  " N2 " + numLoD2 + " V " + visibleAttractions.Count;
            //        //********************************************************************************************************************************************************************************************************

            //        //debugTextObject.GetComponent<Text>().text = attractions[0].AttractionBackground.GetComponentsInChildren<Component>().Length + "";


            //        try
            //        {
            //            //arCamera.transform.position = arCamera.transform.position + arCamera.transform.forward;
            //            // Camera.main.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 10);
            //        }
            //        catch (Exception ex)
            //        {
            //            debugTextObject.GetComponent<Text>().text = ex.Message;
            //        }


            //        //attractions.Sort();
            //        //debugTextObject.GetComponent<Text>().text = attractions[0].Distance + " " + attractions[1].Distance + " " + attractions[2].Distance;
            //}

        }
                catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Move";
        }

        yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(1);
    }
    #endregion

    #region AugmentedScript coordinateFetching
    IEnumerator GetCoordinates()
    {

        bool running = true;

        //while true so this function keeps running once started.
        while (running)
        {
            Input.location.Start(1.0f, 1.0f);

            // check if user has location service enabled
            if (!Input.location.isEnabledByUser)
                yield break;

            Input.compass.enabled = true;

            // Wait until service initializes
            int maxWait = 10;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                //Thread.Sleep(1000);

                yield return new WaitForSeconds(1);
                //debugTextObject.GetComponent<Text>().text = "ASDF " + maxWait;

                maxWait--;
            }


            // Service didn't initialize in 20 seconds
            if (maxWait < 1)
            {
                print("Timed out");
                //debugTextObject.GetComponent<Text>().text = "2.2!";
                yield break;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                print("Unable to determine device location");
                //debugTextObject.GetComponent<Text>().text = "4!";
                yield break;
            }
            else
            {
                Debug.Log("Location can be retrieved!", distanceTextObject);
                //debugTextObject.GetComponent<Text>().text = "5!";

                // Access granted and location value could be retrieved
                print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

                //if original value has not yet been set save coordinates of player on app start
                if (setOriginalValues)
                {
                    originalLatitude = Input.location.lastData.latitude;
                    originalLongitude = Input.location.lastData.longitude;
                    setOriginalValues = false;
                }

                //overwrite current lat and lon everytime
                currentLatitude = Input.location.lastData.latitude;
                currentLongitude = Input.location.lastData.longitude;

                //calculate the distance between where the player was when the app started and where they are now.
                Calc(originalLatitude, originalLongitude, currentLatitude, currentLongitude);

            }

            // Collision Handling

        }

        Input.location.Stop();
    }
    #endregion

    #region AugmentedScript start
    void Start()
    {
        try { 

            if (showLowestLOD || showOnlyIcons || showOnlyMid)
            {
                showAreas = false;
                showLOD = false;
            }

            if (showLowestLOD)
            {
                showOnlyIcons = false;
                showOnlyMid = false;
            }

            if (showOnlyIcons)
            {
                showLowestLOD = false;
                showOnlyMid = false;
            }

            if (showOnlyMid)
            {
                showLowestLOD = false;
                showOnlyIcons = false;
            }

            //if (showOnlyIcons) showLOD = true;

            startTime = Time.time;
            attractions = new List<Attraction>();
            areas = new List<Area>();
            visibleAttractions = new List<int>();
            attrTextues = new List<Texture2D>();

            prevTime = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

            prevTimeLOD = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

            //arCamera = GameObject.FindGameObjectWithTag("arCamera").GetComponent<Camera>();

            /*try
            {
                arCamera = GameObject.FindGameObjectWithTag("arCamera").GetComponent<Camera>();
            }
            catch (Exception ex)
            {
                debugTextObject.GetComponent<Text>().text = ex.Message;
            }*/

            //get distance text reference
            distanceTextObject = GameObject.FindGameObjectWithTag("distanceText");
            debugTextObject = GameObject.FindGameObjectWithTag("debugText");
            debugTextObject.GetComponent<Text>().text = "App started!";

            Debug.Log("App started!", distanceTextObject);

            attractionImage = GameObject.FindGameObjectWithTag("attractionImage").GetComponent<RawImage>();
            attractionBackground = GameObject.FindGameObjectWithTag("attractionBackground").GetComponent<Image>();
            attractionText = GameObject.FindGameObjectWithTag("attractionText").GetComponent<Text>();
            attractionImage = GameObject.FindGameObjectWithTag("attractionImage").GetComponent<RawImage>();
            attractionCanvas = GameObject.FindGameObjectWithTag("attractionCanvas").GetComponent<Canvas>();

            forwardButton = GameObject.FindGameObjectWithTag("forwardButton").GetComponent<Button>();
            reverseButton = GameObject.FindGameObjectWithTag("reverseButton").GetComponent<Button>();

            forwardButton.onClick.AddListener(MoveForward);
            reverseButton.onClick.AddListener(MoveBack);

                //start GetCoordinate() function 
                //StartCoroutine("GetCoordinates");


            //SpriteRenderer iconObject = GameObject.FindGameObjectWithTag("attractionIcon").GetComponent<SpriteRenderer>();
            //iconObject.sprite = Resources.Load("typeIcon/AdventureSprite") as Sprite;
            thrillSVG = GameObject.FindGameObjectWithTag("thrillSVG").GetComponent<SpriteRenderer>();
            adventureSVG = GameObject.FindGameObjectWithTag("adventureSVG").GetComponent<SpriteRenderer>();
            childrenSVG = GameObject.FindGameObjectWithTag("childrenSVG").GetComponent<SpriteRenderer>();

            thrillBackSVG = GameObject.FindGameObjectWithTag("thrillBackSVG").GetComponent<SpriteRenderer>();
            adventureBackSVG = GameObject.FindGameObjectWithTag("adventureBackSVG").GetComponent<SpriteRenderer>();
            childrenBackSVG = GameObject.FindGameObjectWithTag("childrenBackSVG").GetComponent<SpriteRenderer>();
            //iconObject.sprite = adventureSVG.sprite;

            if (showOnlyText || showLowestLOD) {
                //attractionBackground.gameObject.layer = 0;

                attractionBackground.transform.GetChild(3).gameObject.layer = 0;
                attractionBackground.transform.GetChild(4).gameObject.layer = 9;
                //SmallCubeTransform = AttractionBackground.transform.GetChild(4);
            }

            InitAttractions();

            StartCoroutine("MoveLabels");

            if (OclHandling)
            {
                StartCoroutine("OcclusionHandling");
                StartCoroutine("ShiftObjects");
            }

            //initialize target and original position
            targetPosition = transform.position;
            originalPosition = transform.position;

                //collisionThread.Start();

        }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Start";
        }
    }
    #endregion
    
    #region AugmentedScript moveForward
    public void MoveForward()
    {
        try { 
        ColorBlock cb = forwardButton.colors;       

        forwardButtonDown = !forwardButtonDown;

        if (forwardButtonDown)
        {
            cb.normalColor = new Color(27, 88, 94);
            forwardButton.colors = cb;
        }
        else
        {
            cb.normalColor = new Color(63, 179, 192);
            forwardButton.colors = cb;

            //forwardButton.GetComponent<Image>().color = new Color(63, 179, 192);

            if ((currentPosition - prevPosition).magnitude > 10)
            {
                prevPosition = currentPosition;
                distanceMoved = true;
            }
        }
        }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Move Forward";
        }
    }
    #endregion

    #region AugmentedScript moveBack
    public void MoveBack()
    {
        try { 

        reverseButtonDown = !reverseButtonDown;

        if (reverseButtonDown)
        {
            reverseButton.GetComponent<Image>().color = new Color(27, 88, 94);
        }
        else
        {
            reverseButton.GetComponent<Image>().color = new Color(63, 179, 192);

            if ((currentPosition - prevPosition).magnitude > 10)
            {
                prevPosition = currentPosition;
                distanceMoved = true;
            }
        }
        }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Move Back";
        }
    }
    #endregion

    #region AugmentedScript initAttractions
    public void InitAttractions()
    {
        try { 
        string line;
        string[] parts;

        //debugTextObject.GetComponent<Text>().text = "-1";


        Image backgroundRaw = null;
        Texture2D attrImage = null;
       

        try
        {
            TextAsset attractionsTextFile = Resources.Load("attractions") as TextAsset;
            string[] textFromFile = attractionsTextFile.text.Split('\n');

            Canvas canvasRaw;            

            Texture2D waitTexture;

            for (int i = 0; i < textFromFile.Length; i++)
            {
                line = textFromFile[i];
                
                parts = line.Split(',');
                          

                canvasGameObject = new GameObject();
                canvasGameObject.transform.position = new Vector3(0, 0, 80);
                canvasGameObject.AddComponent<Canvas>();

                canvasRaw = canvasGameObject.GetComponent<Canvas>();
                canvasRaw.renderMode = RenderMode.WorldSpace;
                canvasRaw.scaleFactor = 0.220784f;
                canvasGameObject.AddComponent<CanvasScaler>();
                canvasGameObject.AddComponent<GraphicRaycaster>();

                canvasRaw.transform.parent = canvasGameObject.transform;


                backgroundRaw = Image.Instantiate(attractionBackground);


                    /*RawImage subImage = backgroundRaw.transform.GetComponentsInChildren<RawImage>()[3];
                    RawImage newSubImage;
                    Texture2D subImageTexture;

                    int subImagesCount = 5;
                    Vector3 startPos = subImage.transform.position - new Vector3(10 * (subImagesCount/2), 0, 0);
                    for (int u = 0; u < subImagesCount; u++)
                    {    
                        newSubImage = RawImage.Instantiate(subImage);

                        var subImageColor = newSubImage.color;
                        subImageColor.a = 1.0f;
                        newSubImage.color = subImageColor;


                        //subImageTexture = Resources.Load("icons/" + (100 - int.Parse(parts[6]))) as Texture2D;
                        subImageTexture = Resources.Load("icons/" + (u * 20)) as Texture2D;
                        newSubImage.texture = subImageTexture;


                        newSubImage.transform.SetParent(backgroundRaw.transform);
                        newSubImage.transform.position = startPos;
                        startPos += new Vector3(10, 0, 0);
                    }*/

                    



                backgroundRaw.transform.position = new Vector3(float.Parse(parts[7]), 0, float.Parse(parts[9]));

                canvasGameObject.transform.position = new Vector3(float.Parse(parts[7]), /*float.Parse(parts[8])*/0, float.Parse(parts[9]));
                               

                backgroundRaw.transform.parent = canvasGameObject.transform;

                backgroundRaw.GetComponentsInChildren<Text>()[0].text = parts[0] + " " + int.Parse(parts[6]) + "min";
                backgroundRaw.GetComponentsInChildren<Text>()[1].text = parts[parts.Length - 1];

                attrImage = Resources.Load(/*textureNames[i]*/parts[5].Trim()) as Texture2D;
                backgroundRaw.GetComponentsInChildren<RawImage>()[1].texture = attrImage;

                waitTexture = Resources.Load("icons/" + (100 - int.Parse(parts[6]))) as Texture2D;
                backgroundRaw.GetComponentsInChildren<RawImage>()[0].texture = waitTexture;


                    if (parts[parts.Length - 1].Trim() == "Adventure")
                    {
                        backgroundRaw.GetComponentsInChildren<SpriteRenderer>()[0].sprite = adventureSVG.sprite;
                        backgroundRaw.GetComponentsInChildren<SpriteRenderer>()[1].sprite = adventureBackSVG.sprite;
                    }
                    else if (parts[parts.Length - 1].Trim() == "Thrill")
                    {
                        backgroundRaw.GetComponentsInChildren<SpriteRenderer>()[0].sprite = thrillSVG.sprite;
                        backgroundRaw.GetComponentsInChildren<SpriteRenderer>()[1].sprite = thrillBackSVG.sprite;
                    }
                    else
                    {
                        backgroundRaw.GetComponentsInChildren<SpriteRenderer>()[0].sprite = childrenSVG.sprite;
                        backgroundRaw.GetComponentsInChildren<SpriteRenderer>()[1].sprite = childrenBackSVG.sprite;
                    }
                        


                backgroundRaw.GetComponentsInChildren<Component>()[12].name = parts[0] + "LargeCube";
                backgroundRaw.GetComponentsInChildren<Component>()[16].name = parts[0] + "SmallCube";

                backgroundRaw.transform.LookAt(new Vector3(0, backgroundRaw.transform.position.y, 0));
                backgroundRaw.transform.Rotate(new Vector3(0, 1, 0), 180.0f);

                attractions.Add(new Attraction(
                        parts[0],
                        parts[1],
                        double.Parse(parts[2]),
                        double.Parse(parts[3]),
                        int.Parse(parts[6]),
                        parts[5],
                        backgroundRaw,
                        canvasGameObject
                    ));


                    attractions[i].svgRenderer = attractions[i].AttractionBackground.GetComponentsInChildren<SpriteRenderer>()[0];
                    attractions[i].svgBackRenderer = attractions[i].AttractionBackground.GetComponentsInChildren<SpriteRenderer>()[1];



                    //float greyValue = Math.Abs(attractions[i].WaitingTime - 50) * 4.0f + 55;
                    //attractions[i].svgRenderer.color = new Color(greyValue / 255, greyValue/ 255, greyValue / 255, attractions[i].svgRenderer.color.a);

                    

                    // subImage = RawImage.Instantiate(backgroundRaw.transform.GetComponentsInChildren<RawImage>()[3]);


                    //ROTATION              

                    var texTransColor = attractions[i].AttractionImage.color;
                    texTransColor.a = 1.0f;
                    attractions[i].AttractionImage.color = texTransColor;


                    if (showLowestLOD)
                    {
                        var backColor = attractions[i].AttractionBackground.color;
                        backColor.a = 1.0f;
                        attractions[i].AttractionBackground.color = backColor;

                        var textTransColor = attractions[i].AttractionText.color;
                        textTransColor.a = 1.0f;
                        attractions[i].AttractionText.color = textTransColor;
                    } else
                    {
                        var backColor = attractions[i].AttractionBackground.color;
                        backColor.a = 0.0f;
                        attractions[i].AttractionBackground.color = backColor;

                        var textTransColor = attractions[i].AttractionText.color;
                        textTransColor.a = 0.0f;
                        attractions[i].AttractionText.color = textTransColor;
                    }

                    if (showOnlyIcons)
                    {
                        var backColor = attractions[i].AttractionImage.color;
                        backColor.a = 0.0f;
                        attractions[i].AttractionImage.color = backColor;

                        backColor = attractions[i].svgRenderer.color;
                        backColor.a = 1.0f;
                        attractions[i].svgRenderer.color = backColor;

                        backColor = attractions[i].svgBackRenderer.color;
                        backColor.a = 0.0f;
                        attractions[i].svgBackRenderer.color = backColor;                       

                    }

                    if (showOnlyMid)
                    {
                        var backColor = attractions[i].AttractionBackground.color;
                        backColor.a = 0.0f;
                        attractions[i].AttractionBackground.color = backColor;

                        var textTransColor = attractions[i].AttractionText.color;
                        textTransColor.a = 0.0f;
                        attractions[i].AttractionText.color = textTransColor;
                    }

                    if (showOnlyText)
                    {
                        var backColor = attractions[i].AttractionBackground.color;
                        backColor.a = 1.0f;
                        attractions[i].AttractionBackground.color = backColor;

                        var textTransColor = attractions[i].AttractionText.color;
                        textTransColor.a = 1.0f;
                        attractions[i].AttractionText.color = textTransColor;

                        backColor = attractions[i].AttractionImage.color;
                        backColor.a = 0.0f;
                        attractions[i].AttractionImage.color = backColor;

                        backColor = attractions[i].svgRenderer.color;
                        backColor.a = 0.0f;
                        attractions[i].svgRenderer.color = backColor;

                        backColor = attractions[i].svgBackRenderer.color;
                        backColor.a = 0.0f;
                        attractions[i].svgBackRenderer.color = backColor;

                        backColor = attractions[i].WaitImage.color;
                        backColor.a = 0.0f;
                        attractions[i].WaitImage.color = backColor;
                    }


                  
                    attractions[i].Distance = attractions[i].AttractionBackground.transform.position.magnitude;

                    if (showAreas)
                    {

                        if (parts.Length >= 13)
                        {
                            string areaName = parts[10];
                            Area currentArea = new Area(areaName);


                            if (!areas.Contains(currentArea))
                            {
                                areas.Add(currentArea);

                                canvasGameObject = new GameObject();
                                canvasGameObject.transform.position = new Vector3(0, 0, 80);
                                canvasGameObject.AddComponent<Canvas>();

                                canvasRaw = canvasGameObject.GetComponent<Canvas>();
                                canvasRaw.renderMode = RenderMode.WorldSpace;
                                canvasRaw.scaleFactor = 0.220784f;
                                canvasGameObject.AddComponent<CanvasScaler>();
                                canvasGameObject.AddComponent<GraphicRaycaster>();

                                canvasRaw.transform.parent = canvasGameObject.transform;

                                backgroundRaw = Image.Instantiate(attractionBackground);

                                //RECENLT COMMENTED OUT
                                //backgroundRaw.transform.position = new Vector3(80, 0, 600);
                                //canvasGameObject.transform.position = new Vector3(80, /*float.Parse(parts[8])*/0, 600);

                                backgroundRaw.transform.parent = canvasGameObject.transform;

                                backgroundRaw.GetComponentInChildren<Text>().text = parts[10];

                                backgroundRaw.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta = new Vector2(340, 120);
                                //backgroundRaw.GetComponentsInChildren<RawImage>()[1].rectTransform.sizeDelta = new Vector2(300, 100);
                                backgroundRaw.GetComponentsInChildren<RawImage>()[1].rectTransform.sizeDelta = new Vector2(270, 90);
                                backgroundRaw.GetComponentsInChildren<RawImage>()[1].transform.position += new Vector3(0, 2, 0);

                                //debugTextObject.GetComponent<Text>().text = Area.TextureNames[0];


                                attrImage = Resources.Load(Area.TextureNames[areaName.Trim()]) as Texture2D;
                                backgroundRaw.GetComponentsInChildren<RawImage>()[1].texture = attrImage;


                                //waitTexture = Resources.Load("icons/40"/* + (100 - int.Parse(parts[6]))*/) as Texture2D;
                                //backgroundRaw.GetComponentsInChildren<RawImage>()[0].texture = waitTexture;

                                backgroundRaw.GetComponentsInChildren<Component>()[12].name = parts[10] + "LargeCube";
                                backgroundRaw.GetComponentsInChildren<Component>()[16].name = parts[10] + "SmallCube";

                                //RECENLT COMMENTED OUT
                                //backgroundRaw.transform.LookAt(new Vector3(0, backgroundRaw.transform.position.y, 0));
                                //backgroundRaw.transform.Rotate(new Vector3(0, 1, 0), 180.0f);


                                currentArea.AttractionBackground = backgroundRaw;
                                currentArea.CanvasGO = canvasGameObject;
                                currentArea.setValues();


                            }
                            else
                            {
                                foreach (Area a in areas)
                                {
                                    if (a.Equals(currentArea))
                                    {
                                        currentArea = a;
                                    }
                                }
                            }


                            currentArea.SubAttractions.Add(attractions[i]);
                            attractions[i].isAreaExpanded = false;
                            //attractions[i].AttractionArea = currentArea;

                            //debugTextObject.GetComponent<Text>().text += attractions[i].AttractionArea.Name + " ";
                        }
                    }
            }


                if (showAreas)
                {
                    foreach (Area a in areas)
                    {
                        a.hideAttractions();

                        RawImage subImage = a.AttractionBackground.transform.GetComponentsInChildren<RawImage>()[3];
                        RawImage newSubImage;
                        Texture2D subImageTexture;

                        int subImagesCount = a.SubAttractions.Count;
                        Vector3 startPos = subImage.transform.position - new Vector3(10 * (subImagesCount / 2), 0, 0);

                        List<int> waitingTimes = new List<int>();

                        for (int u = 0; u < subImagesCount; u++)
                        {
                            waitingTimes.Add(a.SubAttractions[u].WaitingTime);                  
                        }

                        waitingTimes.Sort();

                        for (int u = 0; u < subImagesCount; u++)
                        {
                            newSubImage = RawImage.Instantiate(subImage);

                            var subImageColor = newSubImage.color;
                            subImageColor.a = 1.0f;
                            newSubImage.color = subImageColor;


                            //subImageTexture = Resources.Load("icons/" + (100 - int.Parse(parts[6]))) as Texture2D;
                            subImageTexture = Resources.Load("icons/" + (100 - waitingTimes[u])) as Texture2D;
                            newSubImage.texture = subImageTexture;


                            newSubImage.transform.SetParent(a.AttractionBackground.transform);
                            newSubImage.transform.position = startPos;
                            startPos += new Vector3(10, 0, 0);
                        }

                        a.calculateValues();
                    }
                }

            //attractionBackground.transform.GetChild(4).name = "ARRRR!!!";

            //debugTextObject.GetComponent<Text>().text = areas[0].AttractionBackground.transform.position + "#" + areas[0].AttractionBackground.transform.position.magnitude;
        }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.Message + " FirstInit";
        }


        attractionBackground.transform.position += new Vector3(0, 1000, 0);

        int beforeHandlung = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

        //InitialOcclusionHandling();

        //debugTextObject.GetComponent<Text>().text = ((DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) - beforeHandlung) + " LLL";

    }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Init";
        }
    }
    #endregion

    #region AugmentedScript InitialOcclusionHandling
    private void InitialOcclusionHandling()
    {

        try
        {
            attractions.Sort();

            Rect rect;
            RectTransformData currentRectTransform;


            RaycastHit hitt1, hitt2, hitt3, hitt4;

            Vector3[] currentCorners = new Vector3[4];

            float maxHeightShift = 0.0f;
            Vector3 directionTop = new Vector3(), directionBottom, endPoint = new Vector3();
            Vector3 currentLabelPosition;
            Vector3 currentNormalVector;

            Plane hitPlane;
            float distValue;
            Ray collisionRay;

            Vector3 currentEndPoint = new Vector3(0, -10000, 0);
            RaycastHit currentHit = new RaycastHit();

            string howManyHits = "";


            string outMessage = "", aaa = "";
            bool isSthBlocking = true;

            attractions[29].AttractionBackground.transform.position += new Vector3(0, 50, 0);


            //attractions[0].AttractionBackground.transform.position += new Vector3(0, 300, 0);

            //Pirates 26 - Big Thunder 29 - Beaver 24
            //Shooting 2 - Pooh 5 - Omnibus 16
            //Enchanted Tiki 1 - Tom Sawyer - 19

            for (int i = 1; i < attractions.Count; i++)
            {
                if (attractions[i].Name == "Tom Sawyer Island Rafts")
                {
                    outMessage += "Tom " + i + "|"; //26
                }

                if (attractions[i].Name == "The Enchanted Tiki Room")
                {
                    outMessage += "Tiki " + i + "|"; //29
                }


                /*if (attractions[i].Name == "Omnibus")
                {
                    outMessage += "Omnibus " + i + "|"; //29
                }*/

                //attractions[i].AttractionBackground.transform.position += new Vector3(0, 300, 0);

                isSthBlocking = true;
                //int i = 24;
                //if (i == 24 || i == 29 || i == 14 || i == 26 || i == 2 || i == 5 || i == 16 || i == 19 /*|| i == 1 || i == 19*/)
                //{

                //int j = 0;
                while (isSthBlocking/* && j < i*/)
                {
                    //j++;
                    isSthBlocking = false;

                    currentEndPoint = new Vector3(0, -10000, 0);

                    attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.GetWorldCorners(currentCorners);
                    currentLabelPosition = attractions[i].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.position;
                    currentNormalVector = Vector3.Cross(currentCorners[2] - currentCorners[1], currentCorners[0] - currentCorners[1]);

                    //outMessage += attractions[i].Name + " --- " + currentCorners[0] + "-" + currentCorners[1] + "-" + currentCorners[2] + "-" + currentCorners[3] + "-";
                    //outMessage += System.Environment.NewLine;// + attractions[11].Name;

                    if (Physics.Raycast(Camera.main.transform.position, (currentCorners[0] - Camera.main.transform.position), out hitt1, (currentCorners[0] - Camera.main.transform.position).magnitude - 10, 9))
                    {
                        //howManyHits += " 3 " + hitt3.collider.name + "/" + hitt3.distance + " ";

                        directionTop = hitt1.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
                        directionTop = (directionTop - Camera.main.transform.position).normalized;

                        hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);

                        collisionRay = new Ray(Camera.main.transform.position, directionTop);
                        hitPlane.Raycast(collisionRay, out distValue);
                        endPoint = collisionRay.GetPoint(distValue);

                        //outMessage += " HIT " + hitt1.collider.name;


                        if (endPoint.y > currentEndPoint.y)
                        {
                            currentEndPoint = endPoint;
                            currentHit = hitt1;
                            isSthBlocking = true;
                        }
                    }

                    if (Physics.Raycast(Camera.main.transform.position, (currentCorners[1] - Camera.main.transform.position), out hitt2, (currentCorners[1] - Camera.main.transform.position).magnitude - 10, 9))
                    {
                        //howManyHits += " 3 " + hitt3.collider.name + "/" + hitt3.distance + " ";

                        directionTop = hitt2.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
                        directionTop = (directionTop - Camera.main.transform.position).normalized;

                        hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);


                        collisionRay = new Ray(Camera.main.transform.position, directionTop);
                        hitPlane.Raycast(collisionRay, out distValue);
                        endPoint = collisionRay.GetPoint(distValue);

                        //outMessage += " HIT " + hitt2.collider.name;

                        if (endPoint.y > currentEndPoint.y)
                        {
                            currentEndPoint = endPoint;
                            currentHit = hitt2;
                            isSthBlocking = true;
                        }
                    }

                    if (Physics.Raycast(Camera.main.transform.position, (currentCorners[2] - Camera.main.transform.position), out hitt3, (currentCorners[2] - Camera.main.transform.position).magnitude - 10, 9))
                    {
                        //howManyHits += " 3 " + hitt3.collider.name + "/" + hitt3.distance + " ";

                        directionTop = hitt3.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
                        directionTop = (directionTop - Camera.main.transform.position).normalized;

                        hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);

                        collisionRay = new Ray(Camera.main.transform.position, directionTop);
                        hitPlane.Raycast(collisionRay, out distValue);
                        endPoint = collisionRay.GetPoint(distValue);

                        //outMessage += " HIT " + hitt3.collider.name;

                        if (endPoint.y > currentEndPoint.y)
                        {
                            currentEndPoint = endPoint;
                            currentHit = hitt3;
                            isSthBlocking = true;
                        }

                    }

                    if (Physics.Raycast(Camera.main.transform.position, (currentCorners[3] - Camera.main.transform.position), out hitt4, (currentCorners[3] - Camera.main.transform.position).magnitude - 10, 9))
                    {
                        //howManyHits += " 3 " + hitt3.collider.name + "/" + hitt3.distance + " ";

                        //directionTop = hitt4.collider.gameObject.transform.position + new Vector3(0, (currentHit.collider.gameObject.transform.position.y + (attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2)) - currentHit.point.y, 0);
                        directionTop = hitt4.collider.gameObject.transform.position + new Vector3(0, attractions[0].AttractionBackground.GetComponentsInChildren<RawImage>()[0].rectTransform.sizeDelta.y / 2);
                        directionTop = (directionTop - Camera.main.transform.position).normalized;

                        hitPlane = new Plane(currentNormalVector, currentLabelPosition.magnitude);

                        collisionRay = new Ray(Camera.main.transform.position, directionTop);
                        hitPlane.Raycast(collisionRay, out distValue);
                        endPoint = collisionRay.GetPoint(distValue);

                        //outMessage += " HIT " + hitt4.collider.name;

                        if (endPoint.y > currentEndPoint.y)
                        {
                            currentEndPoint = endPoint;
                            currentHit = hitt4;
                            isSthBlocking = true;
                        }

                    }


                    if (isSthBlocking)
                    {

                        attractions[i].AttractionBackground.transform.position =
                            new Vector3(attractions[i].AttractionBackground.transform.position.x,
                            currentEndPoint.y + (attractions[i].AttractionBackground.rectTransform.sizeDelta.y / 2),
                            attractions[i].AttractionBackground.transform.position.z);


                        /*attractions[i].AttractionBackground.transform.position = 
                            new Vector3(0, 
                            (currentEndPoint.y - attractions[i].AttractionBackground.transform.position.y) + (attractions[i].AttractionBackground.rectTransform.sizeDelta.y / 2), 
                            0);*/
                    }
                }

                //outMessage += i;
            }


            //debugTextObject.GetComponent<Text>().text = outMessage + aaa;
        }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Initial";
        }
    }


    #endregion

    #region AugmentedScript update
    void Update()
    {
        try { 
       
        //debugTextObject.GetComponent<Text>().text = "";
        if (Time.time - startTime >= 6 && initialAfterFiveSeconds)
        {
            setOriginalValues = true;
            initialAfterFiveSeconds = false;

            //firstRegionChange = true;
        }

        int afterTime = DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;


        //linearly interpolate from current position to target position
        //transform.position = Vector3.Lerp(transform.position, targetPosition, speed);
        //rotate by 1 degree about the y axis every frame
        //transform.eulerAngles += new Vector3(0, 1f, 0);

        /* if (Input.GetMouseButtonDown(0))
         {
             RaycastHit hit;
             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

             if (Physics.Raycast(ray, out hit, 10000))
             {*/

        //debugTextObject.GetComponent<Text>().text = hit.transform.parent.gameObject.GetComponentInChildren<Attraction>().Name;
        //if (hit.transform.name == "MyObjectName") Debug.Log("My object is clicked by mouse");
        // }
        //}

    }
        catch (Exception ex)
        {
            debugTextObject.GetComponent<Text>().text = ex.ToString() + " Update";
        }
    }
    #endregion

    #region AugmentedScript gpsCalculation
    //calculates distance between two sets of coordinates, taking into account the curvature of the earth.
    public void Calc(float lat1, float lon1, float lat2, float lon2)
    {
        var R = 6378.137; // Radius of earth in KM
        var dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
        var dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
          Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
          Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        distance = R * c;
        distance = distance * 1000f; // meters
                                     //set the distance text on the canvas

        distanceTextObject.GetComponent<Text>().text = "Distance: " + distance;
        //debugTextObject.GetComponent<Text>().text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " C: " + Input.compass.trueHeading;

        //convert distance from double to float
        float distanceFloat = (float)distance;
        //set the target position of the ufo, this is where we lerp to in the update function
        targetPosition = originalPosition - new Vector3(0, 0, distanceFloat * 60);
        //distance was multiplied by 12 so I didn't have to walk that far to get the UFO to show up closer

        attractionText.fontSize = 24 + Mathf.RoundToInt(distanceFloat * 2);

        attractionBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400 + Mathf.RoundToInt(distanceFloat * 10)); // = 400 + Mathf.RoundToInt(distanceFloat * 10);
        attractionBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100 + Mathf.RoundToInt(distanceFloat * 10)); // = 400 + Mathf.RoundToInt(distanceFloat * 10);

        if (distance > 5)
        {
            attractionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 + Mathf.RoundToInt(distanceFloat * 5));
            attractionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100 + Mathf.RoundToInt(distanceFloat * 5));
        }
        else
        {
            attractionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            attractionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        //attractionCanvas.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
    }

    private float angleFromCoordinate(float lat1, float long1, float lat2, float long2)
    {

        float dLon = (long2 - long1);

        float y = Mathf.Sin(dLon) * Mathf.Cos(lat2);
        float x = Mathf.Cos(lat1) * Mathf.Sin(lat2) - Mathf.Sin(lat1)
                * Mathf.Cos(lat2) * Mathf.Cos(dLon);

        float brng = Mathf.Atan2(y, x);

        brng = (180 / Mathf.PI) * brng;
        //brng = (brng + 360) % 360;
        //brng = 360 - brng; // count degrees counter-clockwise - remove to make clockwise

        return brng;
    }
    #endregion

    // Code from https://answers.unity.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html
    // https://answers.unity.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
    // Project gameobject to screen space
}