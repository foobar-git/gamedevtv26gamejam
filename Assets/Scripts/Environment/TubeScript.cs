using UnityEngine;
using UnityEngine.InputSystem;

public class TubeScript : MonoBehaviour
{
    private bool marioEnteringTube, luigiEnteringTube;
    private bool marioArrivedInTube, luigiArrivedInTube;
    private bool movePlayersTubeToTube, movePlayersOutOfTube;

    [SerializeField] private float speed_movePlayerInOutTubeAnim, speed_movePlayerTubeToTubeAnim, tubeEntryExit_y;

    private GameObject mario, luigi;
    public Transform otherTubeExit;
    public AudioClip soundTube;
    private AudioScript audioScript;
    private Vector3 tubeEntryPosition;
    private Vector3 marioTubeToTubePosition, luigiTubeToTubePosition;
    private Vector3 marioOutOfTubePosition, luigiOutOfTubePosition;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        marioEnteringTube = false;
        luigiEnteringTube = false;
        marioArrivedInTube = false;
        luigiArrivedInTube = false;
        movePlayersTubeToTube = false;
        movePlayersOutOfTube = false;

        speed_movePlayerInOutTubeAnim = 2f;
        speed_movePlayerTubeToTubeAnim = 10f;
        tubeEntryExit_y = 0.7f;
    }

    void Update()
    {
        if (marioEnteringTube) PerPlayerEnterTube(mario, tubeEntryPosition, ref marioEnteringTube, ref marioArrivedInTube);
        if (luigiEnteringTube) PerPlayerEnterTube(luigi, tubeEntryPosition, ref luigiEnteringTube, ref luigiArrivedInTube);

        if (marioArrivedInTube && luigiArrivedInTube && !movePlayersTubeToTube && !movePlayersOutOfTube)
            PreparePlayersForTubeTravel();

        if (movePlayersTubeToTube) MovePlayersTubeToTube(speed_movePlayerTubeToTubeAnim);
        else if (movePlayersOutOfTube) MovePlayersOutOfTube(speed_movePlayerInOutTubeAnim);
    }

    void DisablePlayer(GameObject player)
    {
        player.GetComponent<PlayerController>().playerControlsEnabled = false;
        player.GetComponent<Rigidbody2D>().simulated = false;
    }

    void EnablePlayer(GameObject player)
    {
        player.GetComponent<Rigidbody2D>().simulated = true;
        player.GetComponent<PlayerController>().playerControlsEnabled = true;
    }

    AudioScript GetAudioScript()
    {
        if (audioScript == null && GameManager.Instance != null)
            audioScript = GameManager.Instance.GetComponent<AudioScript>();
        return audioScript;
    }

    void PreparePlayersForTubeTravel()
    {
        marioTubeToTubePosition = new Vector3(otherTubeExit.transform.position.x, otherTubeExit.transform.position.y - tubeEntryExit_y, otherTubeExit.transform.position.z);
        luigiTubeToTubePosition = marioTubeToTubePosition;
        movePlayersTubeToTube = true;
    }

    void PreparePlayersForTubeExit()
    {
        if (GetAudioScript() != null) audioScript.PlayAudio(soundTube);
        marioOutOfTubePosition = new Vector3(otherTubeExit.transform.position.x, otherTubeExit.transform.position.y + tubeEntryExit_y, otherTubeExit.transform.position.z);
        luigiOutOfTubePosition = marioOutOfTubePosition;
    }

    void PerPlayerEnterTube(GameObject player, Vector3 targetPos, ref bool entering, ref bool arrived)
    {
        float dt = speed_movePlayerInOutTubeAnim * Time.deltaTime;
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, dt);
        if (player.transform.position == targetPos)
        {
            player.SetActive(false);
            entering = false;
            arrived = true;
        }
    }

    void MovePlayersTubeToTube(float speed)
    {
        float dt = speed * Time.deltaTime;

        bool marioArrived = mario.transform.position == marioTubeToTubePosition;
        bool luigiArrived = luigi.transform.position == luigiTubeToTubePosition;

        if (!marioArrived)
            mario.transform.position = Vector3.MoveTowards(mario.transform.position, marioTubeToTubePosition, dt);
        if (!luigiArrived)
            luigi.transform.position = Vector3.MoveTowards(luigi.transform.position, luigiTubeToTubePosition, dt);

        if (marioArrived && luigiArrived)
        {
            movePlayersTubeToTube = false;
            PreparePlayersForTubeExit();
            movePlayersOutOfTube = true;
        }
    }

    void MovePlayersOutOfTube(float speed)
    {
        float dt = speed * Time.deltaTime;

        mario.SetActive(true);
        luigi.SetActive(true);

        bool marioArrived = mario.transform.position == marioOutOfTubePosition;
        bool luigiArrived = luigi.transform.position == luigiOutOfTubePosition;

        if (!marioArrived)
            mario.transform.position = Vector3.MoveTowards(mario.transform.position, marioOutOfTubePosition, dt);
        if (!luigiArrived)
            luigi.transform.position = Vector3.MoveTowards(luigi.transform.position, luigiOutOfTubePosition, dt);

        if (marioArrived && luigiArrived)
        {
            movePlayersOutOfTube = false;
            EnablePlayer(mario);
            EnablePlayer(luigi);
            marioEnteringTube = false;
            luigiEnteringTube = false;
            marioArrivedInTube = false;
            luigiArrivedInTube = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (pc.assignedHand == PlayerController.Hand.Left && !marioEnteringTube && !marioArrivedInTube)
        {
            if (kb.sKey.wasPressedThisFrame)
            {
                mario = pc.gameObject;
                DisablePlayer(mario);
                tubeEntryPosition = new Vector3(transform.position.x, transform.position.y - tubeEntryExit_y, transform.position.z + 1f);
                marioEnteringTube = true;
                if (GetAudioScript() != null) audioScript.PlayAudio(soundTube);
            }
        }
        else if (pc.assignedHand == PlayerController.Hand.Right && !luigiEnteringTube && !luigiArrivedInTube)
        {
            if (kb.downArrowKey.wasPressedThisFrame)
            {
                luigi = pc.gameObject;
                DisablePlayer(luigi);
                tubeEntryPosition = new Vector3(transform.position.x, transform.position.y - tubeEntryExit_y, transform.position.z + 1f);
                luigiEnteringTube = true;
                if (GetAudioScript() != null) audioScript.PlayAudio(soundTube);
            }
        }
    }

} // end of class
