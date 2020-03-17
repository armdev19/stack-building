using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using GoogleMobileAds.Api;

public class TheStack : MonoBehaviour {

    // reclams banners
    //[SerializeField] private string appID = "ca-app-pub-9225307158641311~5866109668";
    //private const string FullScreenBanner = "ca-app-pub-9225307158641311/1300178696";
    //private const string StackBanner = "ca-app-pub-9225307158641311/5495532433";
    //private static int counter = 0;
    //private InterstitialAd ad;

    public Text scoreText;
    public Color32[] gameColors = new Color32[4];
    public Material stackMat;
    public GameObject endPanel;

    //public AudioSource comboAudio;
    public AudioSource comboMakeAudio;
    public AudioSource gameOverAudio;

    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5.0f;
    private const float ERROR_MARGIN = 0.1f;
    private const float STACK_BOUNDS_GAIN = 0.25f;
    private const int COMBO_START_GAIN = 5;

    //private static int counter = -1;

    private GameObject[] theStack;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int stackIndex;
    private int scoreCount = 0;
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;

    private bool isMovingOnX = true;
    private bool gameOver = false;

    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;

    



    private void Start ()
    {
        theStack = new GameObject[transform.childCount];
        //scoreCount = -transform.childCount;
        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
            //scoreCount++;
        }

        stackIndex = transform.childCount - 1;

       // BannerView bannerV = new BannerView(StackBanner, AdSize.Banner, AdPosition.Bottom);
       // AdRequest request = new AdRequest.Builder().Build();
       // bannerV.LoadAd(request);

	}

    private void Awake()
    {
        //MobileAds.Initialize(appID);
        //Instance = this;
    }

    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);

    }
	
	
	private void Update ()
    {
        if (gameOver)
            return;

        if (Input.GetMouseButtonDown(0))
        {

            if (PlaceTile())
            {
                SpawnTile();
                scoreCount++;
                scoreText.text = scoreCount.ToString();
            }
            else
            {
                EndGame ();
            }
        }

        MoveTile();

        // Move the stack
        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);

	}

    private void MoveTile()
    {

        //if (gameOver)
            //return;

        tileTransition += Time.deltaTime * tileSpeed;
        if(isMovingOnX)
            theStack[stackIndex].transform.localPosition = new Vector3 (Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
        else
            theStack[stackIndex].transform.localPosition = new Vector3 (secondaryPosition, scoreCount, Mathf.Sin (tileTransition) * BOUNDS_SIZE);
    }

    private void SpawnTile()
    {
        lastTilePosition = theStack[stackIndex].transform.localPosition;
        stackIndex--;
        if (stackIndex < 0)
            stackIndex = transform.childCount - 1;

        desiredPosition = (Vector3.down) * scoreCount;
        theStack[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
        theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);


        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
    }

    private bool PlaceTile()
    {
        Transform t = theStack[stackIndex].transform;

        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                // cut the tile
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)

                    return false;
                

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                CreateRubble
                    (
                    new Vector3((t.position.x > 0)
                        ? t.position.x + (t.localScale.x / 2)
                        : t.position.x - (t.localScale.x / 2)
                        , t.position.y
                        , t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
                    );
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    
                    stackBounds.x += STACK_BOUNDS_GAIN;
                    if (stackBounds.x > BOUNDS_SIZE)
                        stackBounds.x = BOUNDS_SIZE;
                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                    
                }

                if (combo >= 0)
                {
                    comboMakeAudio.Play();
                }

                combo++;
                //comboMakeAudio.Play();
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
        }
        else
        {
            float deltaZ = lastTilePosition.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                // cut the tile
                combo = 0;
                stackBounds .y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                CreateRubble
                (
                    new Vector3(t.position.x
                    , t.position.y
                    , (t.position.z > 0)
                    ? t.position.z + (t.localScale.z / 2)
                    : t.position.z - (t.localScale.z / 2)),
                    new Vector3 (t.localScale.x, 1, Mathf.Abs (deltaZ))
                    );
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    
                    if (stackBounds.y > BOUNDS_SIZE)
                        stackBounds.y = BOUNDS_SIZE;

                    stackBounds.y += STACK_BOUNDS_GAIN;
                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
                 
                }


                if (combo >= 0)
                {
                    comboMakeAudio.Play();
                }
                combo++;
                
                
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
        }

        secondaryPosition = (isMovingOnX)
            ? t.localPosition.x
            : t.localPosition.z;
        isMovingOnX = !isMovingOnX;

        return true;
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);
        


        for (int i = 0; i < vertices.Length; i++)

        colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);

        mesh.colors32 = colors;


    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)

            return Color32.Lerp(a, b, (t / 0.33f));
        else if (t < 0.66f)
            return Color32.Lerp(b, c, (t - 0.33f) / 0.33f);
        else
            return Color32.Lerp(c, d, (t - 0.66f) / 0.66f);
        
    }
            

    private void EndGame()
    {
        if (PlayerPrefs.GetInt("score") < scoreCount)
            PlayerPrefs.SetInt("score", scoreCount);

        PlaceTile();
        
        gameOverAudio.Play();
        gameOver = true;
       // ad = new InterstitialAd(FullScreenBanner);
       // AdRequest request = new AdRequest.Builder().Build();
        //ad.LoadAd(request);
        endPanel.SetActive(true);
       // ad.OnAdLoaded += OnAdLoaded;
        theStack[stackIndex].AddComponent<Rigidbody>();

        
    }

    public void OnButtonClick(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        
    }

    public void OnAdLoaded(object sender, System.EventArgs args)
    {
        //if (++counter == 4)
       // {
         //   counter = 0;
           // ad.Show();
        //}
    }

    
}
