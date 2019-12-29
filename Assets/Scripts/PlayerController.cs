using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public CharacterController Mover;
    public Camera mainCamera;
    public CinemachineVirtualCamera virtualCamera;

    public float movementSpeed;
    public float rotationSpeed;

    public Weapon W;

    float lastPlayerInput;

    Health health;
    BlackBoard bb;
    Text hpDisplay;

    Vector3 lastMousePosition;
    Vector3 turningTowards;
    Vector3 aimingTowards;
    Vector3 movingTowards;

    public Canvas canvas;
    RawImage hpImage;
    Image hpSprite;
    RectTransform hpRect;
    public Texture2D hpBar;

    Color cTransparent;
    Color hpBack;
    Color hpDeadBorder;
    Color hpFore;
    Color hpBorder;
    
    float targetVelocity;
    Vector3 velocity;
    bool idleSlow;

    bool firingLeft;
    bool firingRight;
    bool holdingRight;
    float holdingTime;
    bool reloading = false;
    bool ESCMenuOpen = false;

    string assetPath = Application.streamingAssetsPath;
    string highscore;

    private void Awake() {
        if (Mover == null) { Mover = GetComponent<CharacterController>(); }
        if (W == null) { W = GetComponentInChildren<Weapon>(); }
        if (virtualCamera == null) { virtualCamera = FindObjectOfType<CinemachineVirtualCamera>(); }

        health = GetComponent<Health>();
        bb = FindObjectOfType<BlackBoard>();
        mainCamera = Camera.main;
        lastMousePosition = Input.mousePosition;

        cTransparent = new Color(0, 0, 0, 0);
        hpBack = new Color(0.5f, 0.1f, 0.2f, 0.8f);
        hpDeadBorder = new Color(0.3f, 0.0f, 0.0f, 0.85f);
        hpFore = new Color(0.4f, 0.8f, 0.4f, 0.8f);
        hpBorder = new Color(0.05f, 0.1f, 0.05f, 0.8f);
        hpBar = new Texture2D(320, 320); //Must be a square
        
        
        int filledSectors = health.hp;
        int totalSectors = health.hpMax;
        int w = hpBar.width;
        int h = hpBar.height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                hpBar.SetPixel(x, y, cTransparent); //Clear pixel first
            }
        }
        canvas = GameObject.Find("PlayerHealth").GetComponent<Canvas>();
        hpDisplay = canvas.GetComponentInChildren<Text>();
        
        hpRect = canvas.GetComponent<RectTransform>();

        hpSprite = canvas.gameObject.AddComponent<Image>();

        hpSprite.sprite = Sprite.Create(hpBar, new Rect(0,0,hpBar.width,hpBar.height), Vector2.zero);

        hpBar.Apply();
        hpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hpBar.width);
        hpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hpBar.height);
        updateHP();
    }

    private void Start() {
        Mover.stepOffset = 0.5f;
    }
    public void buttonSaveAndExit()
    {
        if (bb.bestScore + 1 < PlayerPrefs.GetInt(PlayerPrefs.GetString("highscore_name", "Default"), 50))
        {
            StartCoroutine(Quit());
            return;
        }
        if (bb.scoreNameInput.text != "")
        {
            PlayerPrefs.SetString("highscore_name", bb.scoreNameInput.text);
            PlayerPrefs.SetInt(bb.scoreNameInput.text, bb.bestScore);
        }
        string highscore_name = PlayerPrefs.GetString("highscore_name", "Default");
        bb.scoreText.text = highscore_name + ": " + PlayerPrefs.GetInt(highscore_name, 50);
        PlayerPrefs.Save();

        StartCoroutine(Quit());
    }
    public void buttonSaveAndReturn()
    {
        if (bb.bestScore + 1 < PlayerPrefs.GetInt(PlayerPrefs.GetString("highscore_name", "Default"), 50))
        {
            StartCoroutine(returnToGame());
            return;
        }
        if (bb.scoreNameInput.text != "")
        {
            PlayerPrefs.SetString("highscore_name", bb.scoreNameInput.text);
            PlayerPrefs.SetInt(bb.scoreNameInput.text, bb.bestScore);
        }
        string highscore_name = PlayerPrefs.GetString("highscore_name", "Default");
        bb.scoreText.text = highscore_name + ": " + PlayerPrefs.GetInt(highscore_name, 50);
        PlayerPrefs.Save();
        StartCoroutine(returnToGame());
    }
    IEnumerator returnToGame()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        ESCMenuOpen = false;
        bb.ESCMenuPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }
    IEnumerator Quit()
    {
        PlayerPrefs.Save();
        yield return new WaitForSecondsRealtime(0.1f);
        Application.Quit();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ESCMenuOpen) { buttonSaveAndExit(); }
            if (!PlayerPrefs.HasKey("highscore_name"))
            {
                PlayerPrefs.SetString("highscore_name", "Default");
                PlayerPrefs.SetInt("Default", 50);
                PlayerPrefs.Save();
            }
            string highscore_name = PlayerPrefs.GetString("highscore_name");
            bb.scoreText.text = highscore_name + ": " + PlayerPrefs.GetInt(highscore_name);
            bb.txtBestScore.text = ""+ bb.bestScore;
            ESCMenuOpen = true;
            bb.ESCMenuPanel.SetActive(true);
            
            Time.timeScale = 0.01f;
            PlayerPrefs.Save();
        }
        if (ESCMenuOpen)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                buttonSaveAndReturn();
            }
            return; //Don't do other stuff while ESC menu is open
        }
        if (health.dying) { return; } //Don't do anything if you are currently dying

        //Update things that always happen regardless of player input
        TurnPlayer();

        if (Time.time - lastPlayerInput > 300f) { //Player has been afk for awhile, put it into slowmotion
            Time.timeScale = 0.05f;
            idleSlow = true;
        }


        /* Don't bother with this until I actually care when the player is idle
        //Is the player doing anything at all? If no, don't bother with anything else
        if (!Input.anyKey && //No keys are being held down
            !Input.anyKeyDown && //No key has been pressed down this frame
            Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) < 0.05f && //The mousewheel is not being spun. Actual value range is usually -0.2 to 0.2
            Mathf.Abs((Input.mousePosition - lastMousePosition).sqrMagnitude) < 5.0f) //The mouse position has not changed significantly
        { return; } //Player hasn't done anything, exit
        */
        lastPlayerInput = Time.time; //Set the timer whenever player does something, so we can tell when they've been idle
        if (idleSlow) { Time.timeScale = 1.0f; } //If they just got back then set time back to normal
        

        if (Mathf.Abs((Input.mousePosition - lastMousePosition).sqrMagnitude) > 5.0f) //Mouse has moved, where is the player looking?
        {
            lastMousePosition = Input.mousePosition; //Set new position only after the mouse has moved a total distance away from previous point
        }

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        //Layer 11 is MouseCast

        if (Physics.Raycast(ray, out hit, 150.0f, 1 << LayerMask.NameToLayer("MouseCast"))) {
            //if ((turningTowards - hit.point).sqrMagnitude > 5.0f) { //Player looked a new direction, idgaf right now
            //    firingLeft = false;
            //    firingRight = false;
            //}
            turningTowards = hit.point;
            turningTowards.y = 0.0f;
            aimingTowards = turningTowards;
            //movingTowardsPrefab.transform.position = turningTowards;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0.0f) //Scrolling Up
        {
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance -= 0.5f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0.0f) //Scrolling Down
        {
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance += 0.5f;
        }

        if (Input.GetMouseButton(0)) {
            if (Time.time - lastPlayerInput < 0.02f) {
                firingLeft = true;
                firingRight = false;
                holdingRight = false;
            }
        }
        if (Input.GetMouseButtonDown(1) && W.chargeCount < W.chargeMax) {
            firingLeft = false;
            firingRight = false;
            holdingRight = true;
            holdingTime = Time.time;
        }
        if (!Input.GetMouseButton(1))
        {
            holdingRight = false;
        }
        if (Input.GetMouseButtonUp(1) || (Input.GetMouseButton(1) && W.chargeCount >= W.chargeMax))
        {
            if (!reloading) //Only fire if you weren't just reloading
            {
                firingLeft = false;
                firingRight = true;
                holdingRight = false;
            }
            else
            {
                reloading = false;
            }
        }
        if (Input.GetMouseButton(1) && !firingRight)
        {
            holdingRight = true;
        }
        Vector3 myPosition = this.transform.position;
        myPosition.y = 0;
        Vector3 myForward = this.transform.forward;
        myForward.y = 0;
        Vector3 myTarget = aimingTowards;
        myTarget.y = 0;

        if (firingLeft) {
            if (Vector3.Dot(myForward, (myTarget - myPosition).normalized) > 0.99f) {
                W.firstAttack();
                firingLeft = false;
            }
        } else if (firingRight) {
            if (Vector3.Dot(myForward, (myTarget - myPosition).normalized) > 0.99f) {
                W.secondAttack();
                firingRight = false;
            }
        }else if (holdingRight && (holdingTime + 1f) < Time.time) //TODO: reload time shouldn't be hardcoded
        {
            W.secondReload();
            holdingTime = Time.time;
            reloading = true;
        }


        movingTowards = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) { //TODO: Make a configurable menu for inputs
            movingTowards += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S)) {
            movingTowards += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A)) {
            movingTowards += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            movingTowards += new Vector3(1, 0, 0);
        }
        if (!V3Equal(Vector3.zero, movingTowards)) {
            movingTowards *= movementSpeed;
            //movingTowards += Vector3.up * -5.0f;
            Mover.Move(movingTowards * Time.deltaTime);
        }
        
        RaycastHit groundedHit;
        Ray r = new Ray(this.transform.position, -Vector3.up);
        if (!Physics.Raycast(r, out groundedHit,  0.76f, 1 << LayerMask.NameToLayer("MouseCast"))) {
            Debug.Log("Raycast did not hit ground");
            this.transform.Translate(Vector3.up * -0.25f);
        }

        //Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }
    public void buttonCloseESCMenu()
    {
        if (bb.ESCMenuPanel != null)
        {
            bb.ESCMenuPanel.SetActive(false);
            Time.timeScale = 1.0f;
            ESCMenuOpen = false;
        }
        else
        {
            Debug.Log("ESCMenuPanel apparently doesn't exist.");
        }
    }

    public void TurnPlayer() {
        //Get angle between points by subtracting current position from target position
        Vector3 direction = (turningTowards - this.transform.position).normalized;
        direction.y = 0.0f; //Third dimension unnecessary

        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, //Origin
                                                           Quaternion.LookRotation(direction, Vector3.up), //Target
                                                           Time.deltaTime * rotationSpeed); //Speed multiplied by time so rotation is consistent over time
    }
    public bool V3Equal(Vector3 a, Vector3 b) {
        return Vector3.SqrMagnitude(a - b) < 0.000001;
    }
    public void updateHP()
    {
        hpUpdating = true;
        StartCoroutine(hpUpdate());
    }
    bool hpUpdating = false;
    public IEnumerator hpUpdate()
    { //Graphical update
        hpUpdating = false;
        int filledSectors = health.hp;
        int totalSectors = health.hpMax;
        int w = hpBar.width;
        int h = hpBar.height;

        int cx = w / 2;
        int cy = w / 2;
        float percent = (float)filledSectors / (float)totalSectors;
        hpDisplay.text = filledSectors + "/" + totalSectors;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (hpUpdating) { StopCoroutine(hpUpdate()); }
                int fromCenterX = Mathf.Abs(cx - x);
                int fromCenterY = Mathf.Abs(cy - y);

                int dist = (int)Mathf.Sqrt((fromCenterX * fromCenterX) + (fromCenterY * fromCenterY));
                hpBar.SetPixel(x, y, new Color(0, 0, 0, 0));
                if (dist < w / 15)
                {
                    hpBar.SetPixel(x, y, hpBorder);
                }
                else if (dist < w / 13)
                {
                    hpBar.SetPixel(x, y, hpDeadBorder);//Border around center circle
                }
                else if (dist < w / 4)
                {
                    if (filledSectors < totalSectors)
                    { //Player is damaged
                        percent = (float)filledSectors / (float)totalSectors;
                        float endAngle = 360f * percent;
                        float angle = Mathf.Atan2(y - cy, x - cx);
                        angle *= Mathf.Rad2Deg;
                        angle += 180f;

                        if (angle >= 0 && angle <= endAngle)
                        {
                            hpBar.SetPixel(x, y, hpFore);
                        }
                        else
                        {
                            hpBar.SetPixel(x, y, hpBack); //if not healthy, add damaged color
                        }
                        for (int i = 0; i <= totalSectors; i++)
                        {
                            float ao = 360f / totalSectors;
                            float sa = i * ao;

                            if (dist < w / 8)
                            {
                                if (angle >= sa - 3f &&
                                    angle <= sa + 3f)
                                {
                                    if (i > filledSectors && i < totalSectors)
                                    {
                                        hpBar.SetPixel(x, y, hpDeadBorder);
                                    }
                                    else
                                    {
                                        hpBar.SetPixel(x, y, hpBorder);
                                    }
                                }
                            }
                            else if (dist < w / 10)
                            {
                                if (angle >= sa - 2f &&
                                    angle <= sa + 2f)
                                {
                                    if (i > filledSectors && i < totalSectors)
                                    {
                                        hpBar.SetPixel(x, y, hpDeadBorder);
                                    }
                                    else
                                    {
                                        hpBar.SetPixel(x, y, hpBorder);
                                    }
                                }
                            }
                            else
                            {
                                if (angle >= sa - 2f &&
                                    angle <= sa + 2f)
                                {
                                    if (i > filledSectors && i < totalSectors)
                                    {
                                        hpBar.SetPixel(x, y, hpDeadBorder);
                                    }
                                    else
                                    {
                                        hpBar.SetPixel(x, y, hpBorder);
                                    }
                                }
                            }
                        }

                    }
                    else
                    { //player is at full health
                        hpBar.SetPixel(x, y, hpFore); //All green

                        //Setup borders
                        for (int i = 0; i <= totalSectors; i++)
                        {
                            float ao = 360f / totalSectors;
                            float sa = i * ao;
                            float angle = Mathf.Atan2(y - cy, x - cx);
                            angle *= Mathf.Rad2Deg;
                            angle += 180f;

                            if (dist < w / 8)
                            {
                                if (angle >= sa - 3f &&
                                    angle <= sa + 3f)
                                {
                                    hpBar.SetPixel(x, y, hpBorder);
                                }
                            }
                            else if (dist < w / 10)
                            { //Use more pixels closer to the center
                                if (angle >= sa - 2f &&
                                    angle <= sa + 2f)
                                {
                                    hpBar.SetPixel(x, y, hpBorder);
                                }
                            }
                            else
                            {
                                if (angle >= sa - 2f &&
                                    angle <= sa + 2f)
                                {
                                    hpBar.SetPixel(x, y, hpBorder);
                                }
                            }
                        }
                    }
                }
                else if (dist <= w / 3.9)
                {
                    hpBar.SetPixel(x, y, hpBorder);
                }
            }
            bool flip = false;
            if (flip)
            {
                yield return new WaitForEndOfFrame();
                flip = false;
            }
            else { flip = true; }
        }
        hpBar.Apply();
        //hpImage.texture = hpBar;
    }
    string readTo = "";
    void readTextFile(string file_path)
    {
        StreamReader reader = new StreamReader(file_path);
        readTo = "";
        while (!reader.EndOfStream)
        {
            string inp_ln = reader.ReadLine();
            readTo += inp_ln;

        }
        reader.Close();
    }
    void writeTextFile(string file_path, string toSave, bool overWrite)
    {
        if (File.Exists(file_path))
        {
            if (overWrite)
            {
                File.Delete(file_path);
            }
            else
            {
                return;
            }
        }
        StreamWriter writer = new StreamWriter(file_path, true);
        writer.Write(toSave);
        writer.Close();
    }
}
