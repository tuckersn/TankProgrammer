module FS.NodeGraph


open Godot
open FS.Player


type SlotValueList = List<Choice<float32, string, bool>>

type Slot =
    | Number of float32
    | Percentage of float32
    | Text of string
    | Bool of bool
    | List of SlotValueList


    static member toString (slot: Slot): string =
        match slot with
            | Number(_) -> "number"
            | Percentage(_) -> "percentage"
            | Text(_) -> "text"
            | Bool(_) -> "bool"
            | List(_) -> "list"

    static member value(slot) =
        match slot with
            | Number value -> Choice1Of4(value)
            | Percentage value -> Choice1Of4(value)
            | Text value -> Choice2Of4(value)
            | Bool value -> Choice3Of4(value)
            | List value -> Choice4Of4(value)

    static member percentage(slot: Slot) =
        match Slot.value(slot) with
            | Choice1Of4(value) -> match value with
                | v when v > 1f -> 1f
                | v when v < -1f -> -1f
                | _ -> value 
            | _ -> raise (System.ArgumentException("Wrong type!"));

    static member text(slot: Slot) =
        match Slot.value(slot) with
            | Choice2Of4(value) -> value
            | _ -> raise (System.ArgumentException("Wrong type!"));

    member this.slotType() =
        match this with 
            | Number(_) -> 0
            | Percentage(_) -> 0
            | Text(_) -> 1
            | Bool(_) -> 2
            | List(_) -> 3





  





type NodeGraph() as _this =
    inherit Control()



type Node() as _this =
    let Test = "test"

[<AbstractClass>]
type NodeGraphNode() as _this = 
    inherit GraphNode()
    (*
        For recursive execution mainly
        Key is the slot idx in this node
        Value goes as follows:
            uint = the slot idx in the other node
            NodeGraphNode = the other node
    *)
    let mutable inputs: Map<int, (int * NodeGraphNode) > = Map [];

    (*
        This is only for retrieving the value of this Node's slots
        it is set in Execute by each child, the key is the slot's index
        Key is the slot idx
        Value of the slot
    *)
    let mutable outputs: Map<int, Slot> = Map[];

    member this.SetInputs(newInputs: Map<int, (int * NodeGraphNode) >) =
        inputs <- newInputs

    member this.GetInputs(): Map<int, (int * NodeGraphNode) > =
        inputs
        

    member this.SetOutputs(newOutputs: Map<int, Slot>) =
        outputs <- newOutputs

    member this.GetOutputs(): Map<int, Slot> =
         outputs

    abstract member Execute: unit -> unit;
    member public this.ExecuteInputs() =
        //GD.Print("INPUTS:", this.inputs.Count, this.inputs);
        for input in inputs do
            let (idx, node) = input.Value;
            node.ExecuteInputs()
            node.Execute()
        ()
        


    // https://youtu.be/ZD9X3uvyWmg
    member this.CloseRequest() =
        _this.QueueFree();

    // https://youtu.be/ZD9X3uvyWmg
    member this.ResizeRequest(newMinSize: Vector2) =
        this.RectSize = newMinSize |> ignore
        _this.RectSize = newMinSize |> ignore
        this.RectMinSize = newMinSize |> ignore
        this.SetSize(newMinSize)


type NodeEditor() as _this =
    inherit GraphEdit()

    [<Export>]
    let index = 0u;

    override this._Ready() =
        GD.Print("NODE EDITOR READY");

    (*
        Called when an input slot is connected
        to an output slot.
    *)
    member this.FSConnect(fromName: string, fromIdx: int, toName: string, toIdx: int) =
        GD.Print("CONNECTION IN FS");

        let fromNode = this.GetNode<NodeGraphNode>(new NodePath(fromName));
        let toNode = this.GetNode<NodeGraphNode>(new NodePath(toName));


        if toNode.GetInputs().ContainsKey(toIdx) then
            let (otherIdx, otherFromNode) = toNode.GetInputs().Item(toIdx);
            this.FSDisconnect(otherFromNode.Name, otherIdx, toName, toIdx);
            // if it's a different node we need to connect that new node
            GD.Print("TO IDX: ", toIdx, toNode.Name, " FROM IDX: ", fromIdx, fromNode.Name, " OTHER IDX: ", otherIdx, otherFromNode.Name);
            if not(fromIdx = otherIdx && fromNode.Name = otherFromNode.Name) then
                this.ConnectNode(fromName, fromIdx, toName, toIdx) |> ignore
                toNode.SetInputs(toNode.GetInputs().Add(toIdx, (fromIdx, fromNode))) |> ignore;
        else
            this.ConnectNode(fromName, fromIdx, toName, toIdx) |> ignore
            toNode.SetInputs(toNode.GetInputs().Add(toIdx, (fromIdx, fromNode))) |> ignore;
        

    member this.FSDisconnect(fromName: string, fromIdx: int, toName: string, toIdx: int) =
        let toNode = this.GetNode<NodeGraphNode>(new NodePath(toName));
        let fromNode = this.GetNode<NodeGraphNode>(new NodePath(fromName));

        toNode.SetInputs(toNode.GetInputs().Remove(toIdx)) |> ignore;
        toNode.SetOutputs(fromNode.GetOutputs().Remove(toIdx)) |> ignore;
        this.DisconnectNode(fromName, fromIdx, toName, toIdx);

    

(*
    PLAYER CONTROLS
*)
module PlayerControls =
    type NodeGraphNodePlayerInput() as _this = 
        inherit NodeGraphNode()

        
        override this._Ready() =
            this.Title = "Player Input" |> ignore

        override this.Execute() =
            ()

    type NodeGraphNodePlayerOutput() as _this = 
        inherit NodeGraphNode()

        let mutable movementSpeed: Slot = Slot.Percentage(0f);
        let mutable turningSpeed: Slot = Slot.Percentage(0f);
        let mutable name: Slot = Slot.Text("");

        member public this.GetMovementSpeed() =
            movementSpeed

        member public this.GetTurningSpeed() =
            turningSpeed

        member public this.GetName() =
            name

        override this.Execute() =

            if this.GetInputs().ContainsKey(0) then 
                let (mIdx, mNode) = this.GetInputs().Item(0);
                movementSpeed <- mNode.GetOutputs().Item(mIdx);
            else
                movementSpeed <- Slot.Percentage(0f)
                
            if this.GetInputs().ContainsKey(1) then 
                let (tIdx, tNode) = this.GetInputs().Item(1);
                turningSpeed <- tNode.GetOutputs().Item(tIdx);
            else
                turningSpeed <- Slot.Percentage(0f)

            if this.GetInputs().ContainsKey(4) then 
                let (nIdx, nNode) = this.GetInputs().Item(2);
                name <- nNode.GetOutputs().Item(nIdx);
            else
                name <- Slot.Text("")

            
            
            
    

        override this._Ready() =
            this.Title = "Player Output" |> ignore
        
(*
    CONVERSION OPERATORS
*)
module Conversion = 
    type NodeGraphNodeNumberToPercentage() as _this =
        inherit NodeGraphNode()

        [<Export>]
        let outOf: float32 = 1f;

        override this.Execute() =
            ()


(*
    CONSTANTS
*)
module Constant =
    type NodeGraphNodeNumber() as _this =
        inherit NodeGraphNode()

        [<Export>]
        let value = 0f;

        member public this.Value() = value;

        override this._Ready() =
            base._Ready()
            this.Value() = value |> ignore
            this.Execute()

  
        override this.Execute() =
            this.SetOutputs(this.GetOutputs().Add(0, Slot.Number(value))) |> ignore

    [<Tool>]
    type NodeGraphNodeComment() as _this = 
        inherit NodeGraphNode()
    
        let mutable labelNode: Option<RichTextLabel> = None;
        let mutable comment: string = "";

        [<Export(PropertyHint.MultilineText)>]
        member this.CommentText
            with get() = 
                GD.Print("READ:", comment)
                comment
            and set(value: string) = 
                comment <- value
                match labelNode with
                    | Some(node) -> node.Text <- value
                    | None -> 
                        let label = _this.GetChild(0);
                        match label with
                            | :? RichTextLabel as l ->                        
                                l.Text <- value
                                labelNode <- Some(l)
                            | _ -> ()

        member public this.SetLabel(label: RichTextLabel) =
            labelNode <- Some(label)
    
        override this._Ready() =
            labelNode <- Some(_this.GetNode<RichTextLabel>(new NodePath("Label")));
            labelNode.Value.Text <- comment
            this.Comment <- true

        override this.Execute() =
                   ()

  
(*
    SWITCH STATEMENTS
*)
module Switch =
    
    [<AbstractClass>]
    type NodeGraphBaseSwitch() as _this =
        inherit NodeGraphNode()

        let mutable condition = false;

        member public this.SetCondition(newCondition: bool) =
            condition <- newCondition

        member public this.GetCondition() =
            condition

        abstract member TrueExecute: unit -> unit;
        abstract member FalseExecute: unit -> unit;
        abstract member ConditionExecute: unit -> unit;

        override this.Execute() =
            this.ConditionExecute()
            match condition with
                | true -> this.TrueExecute()
                | false -> this.FalseExecute()

        member this.ExecuteInputs() =
            ()

        member this.BaseSwitch() = this




    type NodeGraphNodeNumberSwitch() as _this =
        inherit NodeGraphBaseSwitch()
        
        
        override this.TrueExecute() =
            ()

        override this.FalseExecute() =
            ()

        override this.ConditionExecute() =
            ()


(*
    Actual Execution Engine
*)
[<AbstractClass>]
type NodeEngine() as _this =
    inherit Control()

    let frameCount: uint64 = 0UL;

    [<Export>]
    let nodePath: NodePath = new NodePath("GraphEdit");
    let editor = lazy(_this.GetNode<NodeEditor>(nodePath)); 
    let playerInput = lazy(editor.Value.GetNode<PlayerControls.NodeGraphNodePlayerOutput>(new NodePath("PlayerInput")));
    let playerOutput = lazy(editor.Value.GetNode<PlayerControls.NodeGraphNodePlayerOutput>(new NodePath("PlayerOutput")));

    member this.Execute() =
        let po = playerOutput.Value;
        po.ExecuteInputs();
        po.Execute();

        let movementSpeed = Slot.percentage(po.GetMovementSpeed()) * -1f;
        let turningSpeed = Slot.percentage(po.GetTurningSpeed());
        let name = Slot.text(po.GetName());



        (*let jumping =
            match value(po.GetJumping()) with
                | Choice3Of4(value) -> if value then 1f else 0f
                | _ -> raise (System.ArgumentException("Wrong type!"));*)

        Player.Update(
            movementSpeed,
            turningSpeed,
            name
        );

        ()

    override this._PhysicsProcess(delta: float32) =
        frameCount = frameCount + 1UL |> ignore

        if frameCount % 60UL % 4UL = 0UL then
            this.Execute()


