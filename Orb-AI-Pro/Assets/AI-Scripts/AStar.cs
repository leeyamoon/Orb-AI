using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Tilemaps;

public class Node : IComparable<Node>
{
    public List<PlayerAction> Actions;
    public PlayerState currentState;
    public double GCost; // Cost from start node
    public double HCost; // Heuristic cost to end node
    public double FCost => GCost + HCost; // Total cost
    public Node Parent; // Parent node in pat

    public Node(List<PlayerAction> action, PlayerState state, double cost, double HeuristicCost)
    {
        Actions = action;
        currentState = state;
        GCost = cost;
        HCost = HeuristicCost;
    }

    public int CompareTo(Node other)
    {
        int compare = FCost.CompareTo(other.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(other.HCost);
        }
        return compare;
    }
}


public class AStar
{
    private PlayerState StartState;
    private Tilemap _bordersGrid;
    private float num_iter;

    public AStar(PlayerState state, Tilemap bordersGrid, float numIter)
    {
        StartState = state;
        _bordersGrid = bordersGrid;
        num_iter = numIter;
    }


    public Node Search(Func<PlayerState, double> heuristic)
    {
        HashSet<string> visited = new HashSet<string>();
        PriorityQueue queue = new PriorityQueue();
        Node StartNode = new Node(new List<PlayerAction>(), StartState, 0, heuristic(StartState));
        queue.Enqueue(new PriorityItem(StartNode,0));
        int num_step = 0;
        Node tempNode = new Node(new List<PlayerAction>(), StartState, 0, heuristic(StartState));

        while (!queue.IsEmpty())
        {
            PriorityItem item = queue.Dequeue();
            if (num_step > num_iter)  //RewardBehavior.Shared().IsCloseToGoal(item.Node.currentState) || 
            {
                return item.Node;
            }
            //  Debug.Log($"{queue.Size()} {num_step}");
            foreach (var successor in GetLegalActions(item.Node.currentState))
            {   
                PlayerState nextSatet = GetNextState(item.Node.currentState, successor);
                // string stateString = StateToStr(nextSatet);
                // if (visited.Contains(stateString))
                // {
                //     continue;
                // }
                // print(heuristic(nextSatet));
                // visited.Add(stateString);
                List<PlayerAction> copyList = new List<PlayerAction>(item.Node.Actions);
                copyList.Add(successor);
                Node successorNode = new Node(copyList, nextSatet, item.Node.GCost + 1, heuristic(nextSatet)); // add penalty for sharp edges
                double newPriority = item.Node.GCost + 1 + heuristic(nextSatet);
                queue.Enqueue(new PriorityItem(successorNode, newPriority));
            }
            num_step++;
            tempNode = item.Node;
            // Debug.Log($"{num_step} {item.Node.Actions.Count}");
        }
        return tempNode;
    }

    private string StateToStr(PlayerState state)
    {
        return "(" + state.posX + ", " + state.posY + ")";
    }

    private List<PlayerAction> GetLegalActions(PlayerState state)
    {
        List<PlayerAction> legal_actions = new List<PlayerAction>();
        var x_movemovent = Enum.GetValues(typeof(AIMovement.XMovement));
        var y_movemovent = Enum.GetValues(typeof(AIMovement.YMovement));
        foreach (AIMovement.XMovement x_move in x_movemovent)
        {
            foreach (AIMovement.YMovement y_move in y_movemovent)
            {
                var action = new PlayerAction(x_move, y_move);
                if(!IsTileAtPosition(GetNextState(state, action).GetAsVec())) 
                    legal_actions.Add(action);
            }
        }
        return legal_actions;
    }
    
    private bool IsTileAtPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = _bordersGrid.WorldToCell(worldPosition);
        return _bordersGrid.HasTile(cellPosition);
    }

    public PlayerState GetNextState(PlayerState currentState, PlayerAction action)
    {
        // Calculate the new X position based on the XMovement action
        int newPosX = currentState.posX;
        switch (action.moveX)
        {
            case AIMovement.XMovement.Left:
                newPosX -= 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.XMovement.Right:
                newPosX += 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.XMovement.Mid:
                // No change to X position
                break;
        }

        // Calculate the new Y position based on the YMovement action
        int newPosY = currentState.posY;
        switch (action.moveY)
        {
            case AIMovement.YMovement.Up:
                newPosY += 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.YMovement.Down:
                newPosY -= 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.YMovement.Mid:
                // No change to Y position
                break;
        }

        // Return the new state with updated positions
        return new PlayerState(newPosX, newPosY);
    }

}


public class PriorityItem : IComparable<PriorityItem>
{
    public Node Node;
    public double Priority_;

    public PriorityItem(Node node,  double priority)
    {
        Node = node;
        Priority_ = priority;
    }

    public int CompareTo(PriorityItem other)
    {
        return Priority_.CompareTo(other.Priority_);
    }
}

public class PriorityQueue
{
    // private List<PriorityItem> heap;
    private SortedSet<PriorityItem> heap;

    public PriorityQueue()
    {
        heap = new SortedSet<PriorityItem>(Comparer<PriorityItem>.Create((a, b) => a.CompareTo(b)));
        // heap = new List<PriorityItem>();
    }

    public void Enqueue(PriorityItem NewItem)
    {   
        // if (heap.Count == 0){
        //     heap.Add(NewItem);
        //     return;
        // }
        // for (int i = 0; i < heap.Count; i++)
        // {
        //     if (NewItem.Priority_ < heap[i].Priority_)
        //     {
        //         heap.Insert(i, NewItem);
        //         return;
        //     }
        //     if (i == heap.Count-1) {
        //         heap.Add(NewItem);
        //         return;
        //     }
        // }
        heap.Add(NewItem);
    }

    public PriorityItem Dequeue()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("The priority queue is empty.");
        }

        var first = heap.Min;
        // var first = heap[0];
        heap.Remove(first);
        return first;
    }

    public bool IsEmpty()
    {
        return heap.Count == 0;
    }

    public void Restart()
    {
        heap.Clear();
        // heap = new List<PriorityItem>();
    }

    public int Size()
    {
        return heap.Count;
    }
}

