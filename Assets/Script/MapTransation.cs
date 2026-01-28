using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class MapTransision : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;
    [SerializeField] float AdditivePos = 2f;




    CinemachineConfiner2D confiner;


    enum Direction { Up, Down, Left, Right , Teleport}
    
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            FadeTransition(collision.gameObject);

            //confiner.BoundingShape2D = mapBoundary;
            //UpdatePlayerPosition(collision.gameObject);

            MapController_Manual.Instance?.HighlightArea(mapBoundary.name);
        }
    }

    async void FadeTransition(GameObject player)
    {
        await ScreenFader.Instance.FadeOut();

        confiner.BoundingShape2D = mapBoundary;
        UpdatePlayerPosition(player);

        await ScreenFader.Instance.FadeIn();

    }







    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;
            return;
        }







        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += AdditivePos;
                break;
            case Direction.Down:
                newPos.y -= AdditivePos;
                break;
            case Direction.Left:
                newPos.x -= AdditivePos;
                break;
            case Direction.Right:
                newPos.x += AdditivePos;
                break;
        }
        player.transform.position = newPos;
    }
}
