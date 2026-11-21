using ImSharp;
using ImSharp.NodeEditor;
using ImSharp.NodesR;

namespace ImSharpOpenTKDemo.Demos;

/// <summary>
/// Interactive demos for all three node-editor libraries in the ImGui family:
/// imnodes (Nelarius), imgui-node-editor (thedmd), and ImNodes (rokups).
/// </summary>
public class NodeEditorDemos : IDisposable
{
    private readonly EditorContextPtr _nodeEditorCtx;
    private readonly CanvasStatePtr _nodesRCanvas;
    private float _nodeGain = 1.5f;
    private bool _initializedLayout;
    private bool _initializedBlueprintLayout;

    // ImNodes (rokups) node state (slot titles must remain valid between BeginSlot and EndSlot)
    private ImVec2 _nodesRPos1 = new(30, 50);
    private ImVec2 _nodesRPos2 = new(240, 80);
    private bool _nodesRSel1;
    private bool _nodesRSel2;
    private readonly unsafe byte* _slotOutUtf8;
    private readonly unsafe byte* _slotInUtf8;

    public unsafe NodeEditorDemos()
    {
        _nodeEditorCtx = ImNodeEditor.CreateEditor();
        _nodesRCanvas = ImNodesR.CreateCanvasState();
        _slotOutUtf8 = Utf8StringHelper.StringToUtf8Ptr("Out");
        _slotInUtf8 = Utf8StringHelper.StringToUtf8Ptr("In");
    }

    public void DrawImNodes(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(980, 20), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(460, 320), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("imnodes (Nelarius)", ref open))
        {
            ImGui.End();
            return;
        }

        ImNodes.BeginNodeEditor();

        if (!_initializedLayout)
        {
            ImNodes.SetNodeGridSpacePos(1, new ImVec2(30, 50));
            ImNodes.SetNodeGridSpacePos(2, new ImVec2(240, 80));
            _initializedLayout = true;
        }

        // Source node
        ImNodes.BeginNode(1);
        ImNodes.BeginNodeTitleBar();
        ImGui.TextUnformatted("Oscillator");
        ImNodes.EndNodeTitleBar();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("Gain", ref _nodeGain, 0f, 3f);
        ImNodes.BeginOutputAttribute(10);
        ImGui.Text("Signal Out");
        ImNodes.EndOutputAttribute();
        ImNodes.EndNode();

        // Destination node
        ImNodes.BeginNode(2);
        ImNodes.BeginNodeTitleBar();
        ImGui.TextUnformatted("Audio Output");
        ImNodes.EndNodeTitleBar();
        ImNodes.BeginInputAttribute(20);
        ImGui.Text("Signal In");
        ImNodes.EndInputAttribute();
        ImGui.Text($"Level: {_nodeGain * 100:0}%");
        ImNodes.EndNode();

        // Live link connecting pin 10 -> pin 20
        ImNodes.Link(100, 10, 20);

        ImNodes.EndNodeEditor();
        ImGui.End();
    }

    public void DrawBlueprintNodeEditor(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(980, 360), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(460, 320), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("imgui-node-editor (thedmd)", ref open))
        {
            ImGui.End();
            return;
        }

        ImNodeEditor.SetCurrentEditor(_nodeEditorCtx);
        ImNodeEditor.Begin("BlueprintEditor");

        var node1 = new NodeId { value_c = 1 };
        var node2 = new NodeId { value_c = 2 };

        if (!_initializedBlueprintLayout)
        {
            ImNodeEditor.SetNodePosition(node1, new ImVec2(30, 50));
            ImNodeEditor.SetNodePosition(node2, new ImVec2(240, 90));
            _initializedBlueprintLayout = true;
        }

        ImNodeEditor.BeginNode(node1);
        ImGui.Text("Input Node");
        ImNodeEditor.BeginPin(new PinId { value_c = 11 }, PinKind.Output);
        ImNodeEditor.PinPivotAlignment(new ImVec2(1.0f, 0.5f));
        ImNodeEditor.PinPivotSize(new ImVec2(0f, 0f));
        ImGui.Text("Out ->");
        ImNodeEditor.EndPin();
        ImNodeEditor.EndNode();

        ImNodeEditor.BeginNode(node2);
        ImGui.Text("Transform Node");
        ImNodeEditor.BeginPin(new PinId { value_c = 21 }, PinKind.Input);
        ImNodeEditor.PinPivotAlignment(new ImVec2(0.0f, 0.5f));
        ImNodeEditor.PinPivotSize(new ImVec2(0f, 0f));
        ImGui.Text("-> In");
        ImNodeEditor.EndPin();
        ImNodeEditor.EndNode();

        ImNodeEditor.Link(
            new LinkId { value_c = 100 },
            new PinId { value_c = 11 },
            new PinId { value_c = 21 },
            new ImVec4(0.4f, 0.8f, 1.0f, 1.0f),
            2.0f);

        ImNodeEditor.End();
        ImGui.End();
    }

    public unsafe void DrawImNodesR(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(20, 620), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(440, 260), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("ImNodes (rokups)", ref open))
        {
            ImGui.End();
            return;
        }

        ImNodesR.BeginCanvas(_nodesRCanvas);

        if (ImNodesR.BeginNode((nint)1, ref _nodesRPos1, ref _nodesRSel1))
        {
            ImGui.Text("Source Node");
            if (ImNodesRNative.ImNodes_BeginOutputSlot(_slotOutUtf8, 1) != 0)
            {
                ImGui.Text("Out");
                ImNodesR.EndSlot();
            }
            ImNodesRNative.ImNodes_Connection((void*)2, _slotInUtf8, (void*)1, _slotOutUtf8);
        }
        ImNodesR.EndNode();

        if (ImNodesR.BeginNode((nint)2, ref _nodesRPos2, ref _nodesRSel2))
        {
            ImGui.Text("Target Node");
            if (ImNodesRNative.ImNodes_BeginInputSlot(_slotInUtf8, 1) != 0)
            {
                ImGui.Text("In");
                ImNodesR.EndSlot();
            }
        }
        ImNodesR.EndNode();

        ImNodesR.EndCanvas();
        ImGui.End();
    }

    public unsafe void Dispose()
    {
        Utf8StringHelper.Free(_slotOutUtf8);
        Utf8StringHelper.Free(_slotInUtf8);
        ImNodesR.DestroyCanvasState(_nodesRCanvas);
        ImNodeEditor.DestroyEditor(_nodeEditorCtx);
    }
}
