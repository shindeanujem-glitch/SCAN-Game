using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using Radishmouse;

public class GameManager : MonoBehaviour
{
    public GameObject flowerPrefab;
    public ButterflyController butterflyController;
    public APIManager apiManager;
    public Transform boardParent; // Parent for spawned flowers
    public LineRenderer connectionLinePrefab;
    public UILineRenderer UILineRenderer;
    private Dictionary<string, FlowerNode> allFlowerNodes = new Dictionary<string, FlowerNode>();
    private List<string[]> currentConnections = new List<string[]>();
    private FlowerNode lastSelectedNode;
    private float minX, minY, maxX, maxY;
    private float bgWidth, bgHeight;
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        StartCoroutine(apiManager.StartGame(OnStartSuccess));
    }

    void OnStartSuccess(APIManager.StartResponse response)
    {
        ClearBoard();
        SpawnFlowers(response);
    }

    void SpawnFlowers(APIManager.StartResponse taskNodes)
    {
        FindMinMaxPositions(taskNodes);
        allFlowerNodes.Clear();

        RectTransform rectTransform = boardParent.GetComponent<RectTransform>();
        bgWidth = rectTransform.rect.width;
        bgHeight = rectTransform.rect.height;

        foreach (var kvp in taskNodes.task)
        {
            Vector3 pos = ConvertCoordinates(kvp.Value.x, kvp.Value.y);
            GameObject flowerGO = Instantiate(flowerPrefab, boardParent);
            flowerGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, pos.y);
            FlowerNode node = flowerGO.GetComponent<FlowerNode>();
            node.label = kvp.Key;
            node.SetLabel(kvp.Key);
            node.OnSelected += OnFlowerSelected;
            node.butterfly = butterflyController.gameObject.transform;
            allFlowerNodes[kvp.Key] = node;
        }
    }

    public void FindMinMaxPositions(APIManager.StartResponse obj)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var pos in obj.task.Values)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;

    }

    Vector3 ConvertCoordinates(float x, float y)
    {

        // Convert backend coords to Unity world space; adjust per your scene scale
        float ux = -(bgWidth/2.5f) + (x-minX)/(maxX-minX)*(bgWidth/2.5f)*1.5f;
        float uy = -(bgHeight/2.5f) + (y-minY)/(maxY-minY)*(bgHeight/2.5f)*1.3f;
        return new Vector3(ux, uy, 0f);
    }

    void OnFlowerSelected(FlowerNode selectedNode)
    {
        if (lastSelectedNode != null)
        {
            //DrawConnectionLine(lastSelectedNode.transform.position, selectedNode.transform.position);

            currentConnections.Add(new string[] { lastSelectedNode.label, selectedNode.label });
        }
        RectTransform rectTransform = selectedNode.GetComponent<RectTransform>();
        Vector2 anchoredPos = rectTransform.anchoredPosition;
        float x = anchoredPos.x;
        float y = anchoredPos.y;
        UILineRenderer.AddPoint(new Vector2(x,y));

        lastSelectedNode = selectedNode;
        // Command butterfly to fly to selected flower
        butterflyController.FlyTo(selectedNode.transform.position);
    }

    void DrawConnectionLine(Vector3 start, Vector3 end)
    {
        LineRenderer lr = Instantiate(connectionLinePrefab, boardParent);
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public void OnSubmitButtonPressed()
    {
        StartCoroutine(apiManager.SubmitConnections(currentConnections.ToList(), OnSubmitResponse));
    }

    void OnSubmitResponse(APIManager.SubmitResponse response)
    {
        if (response.success)
        {
            if (response.status == "done")
            {
                Debug.Log("Game Completed!");
                // Show completion feedback (UI)
            }
            else
            {
                // Load next level
                OnStartSuccess(new APIManager.StartResponse { level = response.level, task = response.task });
                lastSelectedNode = null;
                currentConnections.Clear();
                ClearConnectionLines();
            }
        }
        else
        {
            Debug.LogWarning("Sequence Incorrect. Try Again.");
            // Offer retry logic
            ClearConnectionLines();
            lastSelectedNode = null;
            currentConnections.Clear();
        }
    }

    public void OnClearLastButtonPressed()
    {
        StartCoroutine(apiManager.ClearLastConnection(OnClearLastResponse));
    }

    void OnClearLastResponse(APIManager.ClearResponse response)
    {
        Debug.Log("clear");
        currentConnections = response.connections;
        lastSelectedNode = null;
        ClearConnectionLines();
        RebuildConnectionLines();
    }

    public void OnClearAllButtonPressed()
    {
        StartCoroutine(apiManager.ClearAllConnections(OnClearAllResponse));
    }

    void OnClearAllResponse(APIManager.ClearResponse response)
    {
        currentConnections.Clear();
        lastSelectedNode = null;
        ClearConnectionLines();
    }

    void RebuildConnectionLines()
    {
        for (int i = 0; i < currentConnections.Count; i++)
        {
            FlowerNode startNode = allFlowerNodes[currentConnections[i][0]];
            FlowerNode endNode = allFlowerNodes[currentConnections[i][1]];
            DrawConnectionLine(startNode.transform.position, endNode.transform.position);
            lastSelectedNode = endNode;
        }
    }

    void ClearConnectionLines()
    {
       UILineRenderer.ClearPoints();
    }

    void ClearBoard()
    {
        foreach (Transform child in boardParent)
        {
            if(child.gameObject.name == "LineRenderer") continue;
            else Destroy(child.gameObject);
        }
    }
}
