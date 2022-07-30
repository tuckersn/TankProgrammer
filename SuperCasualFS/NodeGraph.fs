module FS.NodeGraph


open Godot
open FS.Player
open Utils


type SlotType = 
    | Number = 0
    | Percentage = 0
    | Text = 1
    | Bool = 2
    | List = 3

let stringToSlotType(str: string) =
    match str with
        | "number" -> SlotType.Number
        | "percentage" -> SlotType.Percentage
        | "text" -> SlotType.Text
        | "bool" -> SlotType.Bool
        | "list" -> SlotType.List
        | _ -> raise (System.ArgumentException ("Unknown slot type, this is fatal: " + str))

let colorOfSlotType(slotType: SlotType) =
    match slotType with
        | SlotType.Number -> Color.Color8(255uy,0uy,0uy)
        | SlotType.Text -> Color.Color8(0uy,0uy,255uy)
        | SlotType.Bool -> Color.Color8(255uy,0uy,255uy)
        | SlotType.List -> Color.Color8(0uy, 255uy, 0uy)
        | _ -> raise (System.ArgumentException ("Unknown slot type, this is fatal " + slotType.ToString()))

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

    static member bool(slot: Slot) =
        match Slot.value(slot) with
            | Choice3Of4(value) -> value
            | _ -> raise (System.ArgumentException("Wrong type!"));

    member this.slotType() =
        match this with 
            | Number(_) -> SlotType.Number
            | Percentage(_) -> SlotType.Percentage
            | Text(_) -> SlotType.Text
            | Bool(_) -> SlotType.Bool
            | List(_) -> SlotType.List

    member this.booleanOperation(otherSlot: Slot, operator: BooleanOperator) = 
        match operator with
            | BooleanOperator.GreaterThan ->
                (this > otherSlot)
            | BooleanOperator.LessThan ->
                (this < otherSlot)
            | BooleanOperator.Equals ->
                (this = otherSlot)
            | BooleanOperator.GreaterOrEquals ->
                (this >= otherSlot)
            | BooleanOperator.LessOrEquals ->
                (this <= otherSlot)
            | _ -> false

    static member slotTypeToSlot(slotType: SlotType) =
        match slotType with
            | SlotType.Number -> Choice1Of4(Slot.Number)
            | SlotType.Text -> Choice2Of4(Slot.Text)
            | SlotType.Bool -> Choice3Of4(Slot.Bool)
            | SlotType.List -> Choice4Of4(Slot.List)

    static member emptyFromSlotType(slotType: SlotType) =
        match slotType with
            | SlotType.Number -> Slot.Number(0f)
            | SlotType.Text -> Slot.Text("")
            | SlotType.Bool -> Slot.Bool(false)
            | SlotType.List -> Slot.List([])
            | _ -> raise (System.ArgumentException ("Unknown slotType: " + slotType.ToString()))



    static member op_GreaterThan (a: Slot, b: Slot) =
        if a.slotType() = b.slotType() then
            let aValue = Slot.value(a);
            let bValue = Slot.value(b);
            aValue > bValue
        else
            raise (System.ArgumentException ("Invalid slot comparison."))
            


  





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

        Inputs are only in this map if there is a connection,
        use contains key to determine if there is a connection.

        Connections are made by the NodeEditor, that is where
        this will be set.
    *)
    let mutable inputs: Map<int, (int * NodeGraphNode) > = Map [];

    (*
        This is only for retrieving the value of this Node's slots
        it is set in Execute by each child, the key is the slot's index
        Key is the slot idx
        Value of the slot

        These are the results of the Execute method
    *)
    let mutable outputs: Map<int, Slot> = Map[];

    [<Export>]
    let mutable deletable = true;

    member this.Inputs
        with get() = 
            inputs
        and set(newValue: Map<int, (int * NodeGraphNode) >) = 
            inputs <- newValue


    member this.Outputs
        with get() = 
            outputs
        and set(newValue: Map<int, Slot>) = 
            outputs <- newValue

    abstract member Execute: unit -> unit;
    member public this.ExecuteInputs() =
        //GD.Print("INPUTS:", this.inputs.Count, this.inputs);
        for input in inputs do
            let (idx, node) = input.Value;
            node.ExecuteInputs()
            node.Execute()
        ()
        


    // https://youtu.be/ZD9X3uvyWmg
    member this.Close() =
        if deletable then
            this.EmitSignal("close_request");

    member this.CloseRequest() =
        this.QueueFree();

    // https://youtu.be/ZD9X3uvyWmg
    member this.ResizeRequest(newMinSize: Vector2) =
        this.RectSize = newMinSize |> ignore
        _this.RectSize = newMinSize |> ignore
        this.RectMinSize = newMinSize |> ignore
        this.SetSize(newMinSize)
   

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

        let valueLabels = lazy(_this.GetNode<Control>(new NodePath("ValueLabels")));
        let movementLabel = lazy(valueLabels.Value.GetNode<Label>(new NodePath("MovementValue")));
        let turningLabel = lazy(valueLabels.Value.GetNode<Label>(new NodePath("TurningValue")));

        member public this.GetMovementSpeed() =
            movementSpeed

        member public this.GetTurningSpeed() =
            turningSpeed

        member public this.GetName() =
            name

        override this.Execute() =

            
            if this.Inputs.ContainsKey(0) then 
                let (mIdx, mNode) = this.Inputs.Item(0);
                movementSpeed <- mNode.Outputs.Item(mIdx);
            else
                movementSpeed <- Slot.Percentage(0f)
                
            if this.Inputs.ContainsKey(1) then 
                let (tIdx, tNode) = this.Inputs.Item(1);
                turningSpeed <- tNode.Outputs.Item(tIdx);
            else
                turningSpeed <- Slot.Percentage(0f)

            if this.Inputs.ContainsKey(4) then 
                let (nIdx, nNode) = this.Inputs.Item(2);
                name <- nNode.Outputs.Item(nIdx);
            else
                name <- Slot.Text("")

            (*movementLabel.Value.Text <- "(" + Slot.percentage(movementSpeed).ToString() + ")"
            turningLabel.Value.Text <- "(" + Slot.percentage(turningSpeed).ToString() + ")"*)
            
            
    

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
[<Tool>]
module Constant =

    [<AbstractClass>]
    type NodeGraphNodeConstant() as _this =
        inherit NodeGraphNode()

        let mutable slot: Slot = Slot.Number(0f);

        member this.Slot
            with get() = slot
            and set(value: Slot) =
                slot <- value
                this.Execute()

        override this._Ready() =
            base._Ready()
            this.Execute()

        override this.Execute() =
            this.Outputs <- this.Outputs.Add(0, slot)
            let slotType = slot.slotType();
            this.SetSlotColorRight(0, colorOfSlotType(slotType))
            this.SetSlotTypeRight(0, (int) slotType)


    type NodeGraphNodeConstantNumber() as _this =
        inherit NodeGraphNodeConstant()
        member this.SetValue(value: float32) =
            this.Slot <- Slot.Number(value)
    
    type NodeGraphNodeConstantText() as _this =
        inherit NodeGraphNodeConstant()
        member this.SetValue(value: string) =
            this.Slot <- Slot.Text(value)
    
    type NodeGraphNodeConstantBool() as _this =
        inherit NodeGraphNodeConstant()
        member this.SetValue(value: bool) =
            this.Slot <- Slot.Bool(value)



    [<Tool>]
    type NodeGraphNodeComment() as _this = 
        inherit NodeGraphNode()
    
        [<Export>]
        let mutable resizableExport = true;
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
            this.Resizable <- resizableExport

        override this.Execute() =
                   ()



  
module Math =
    
    type NodeGraphNodeRandomNumber() as _this =
        inherit NodeGraphNode()

        let mutable min = 0f;
        let mutable max = 0f;
        let mutable cycles = 0u;
        let mutable maxCycles = 0u;
        let mutable value = Slot.Number(0f);

        static let systemRandom = new System.Random();

        member this.random(min: float32, max: float32) =
            (max - min) * float32(systemRandom.NextDouble()) + min

        [<Export>]
        member this.Min
            with get() = 
                min
            and set(value: float32) = 
                min <- value

        [<Export>]
        member this.Max
            with get() = 
                max
            and set(value: float32) = 
                max <- value

        [<Export>]
        member this.Cycles
            with get() = 
                maxCycles
            and set(value: uint) = 
                maxCycles <- value

        override this._Ready() =
            base._Ready()
            this.Execute()
  
        override this.Execute() =
            cycles <- cycles + 1u
            if cycles > maxCycles then
                cycles <- 0u
                value <- Slot.Number((max - min) * float32(systemRandom.NextDouble()) + min)
            this.Outputs <- this.Outputs.Add(0, value)
            


    [<Tool>]
    type NodeGraphBooleanOperation() as _this =
        inherit NodeGraphNode()

        let mutable slotType: SlotType = SlotType.Number;
        let mutable operator: Utils.BooleanOperator = Utils.BooleanOperator.GreaterThan;
        let mutable bValue = 0f;

        member this.UpdateContents() =
            GD.Print("UPDATING CONTENT: ", slotType);
            match slotType with 
                | SlotType.Number ->
                    this.SetSlotColorLeft(0, colorOfSlotType(slotType))
                    this.SetSlotColorLeft(1, colorOfSlotType(slotType))
                    this.SetSlotTypeLeft(0, int(slotType));
                    this.SetSlotTypeLeft(1, int(slotType));
                | SlotType.Bool ->
                    this.SetSlotColorLeft(0, colorOfSlotType(slotType))
                    this.SetSlotColorLeft(1, colorOfSlotType(slotType))
                    this.SetSlotTypeLeft(0, int(slotType));
                    this.SetSlotTypeLeft(1, int(slotType));
                | SlotType.Text ->
                    this.SetSlotColorLeft(0, colorOfSlotType(slotType))
                    this.SetSlotColorLeft(1, colorOfSlotType(slotType))
                    this.SetSlotTypeLeft(0, int(slotType));
                    this.SetSlotTypeLeft(1, int(slotType));
                | _ -> ()

        [<Export>]
        member this.SlotType
            with get() = slotType
            and set(value: SlotType) = 
                slotType <- value
                this.UpdateContents()

        [<Export>]
        member this.Operator
            with get() = operator
            and set(op: BooleanOperator) =
                operator <- op

        // For use by signals
        member this.SetOperator(op: BooleanOperator) =
            this.Operator <- op

        member this.PopupCB(option: string) =
            this.SlotType <- stringToSlotType(option);
            this.Title <- 
                match this.SlotType with
                    | SlotType.Number -> "Numeric Comparison"
                    | SlotType.Text -> "Text Comparison"
                    | _ ->  "Boolean Comparison"




        override this._Ready() =
            this.UpdateContents()

        override this.Execute() =
            let a = 
                if this.Inputs.ContainsKey(0) then
                    let (aIdx, aNode) = this.Inputs.Item(0)
                    aNode.Outputs.Item(aIdx)
                else
                    Slot.emptyFromSlotType(slotType)

            let b = 
                if this.Inputs.ContainsKey(1) then
                    let (bIdx, bNode) = this.Inputs.Item(1)
                    bNode.Outputs.Item(bIdx)
                else
                    Slot.emptyFromSlotType(slotType)

            this.Outputs <- this.Outputs.Add(2, 
                Slot.Bool(a.booleanOperation(b, operator)))
                    
            
            ()

            

(*
    SWITCH STATEMENTS
*)
module Switch =
    
    [<Tool>]
    type NodeGraphNodeSwitch() as _this =
        inherit NodeGraphNode()

        let mutable slotType: SlotType = SlotType.Number;

        [<Export>]
        member this.SlotType 
            with get() = slotType
            and set(value: SlotType) =
                slotType <- value
                this.SetSlotColorLeft(0, colorOfSlotType(value))
                this.SetSlotColorLeft(1, colorOfSlotType(value))
                this.SetSlotColorRight(0, colorOfSlotType(value))

        member this.GetBool(): bool =
            //GD.Print("GET BOOL: ", _this.Inputs.ContainsKey(2))
            if _this.Inputs.ContainsKey(2) then
                let (cIdx, cNode) = this.Inputs.Item(2)
                GD.Print("GETTING NODE: ", cIdx, Slot.bool(cNode.Outputs.Item(cIdx)))
                if cNode.Outputs.ContainsKey(cIdx) then
                    GD.Print("CONTAIN")
                    Slot.bool(cNode.Outputs.Item(cIdx))
                else
                    false
            else
                false

        override this.Execute() =
            let idx = if this.GetBool() then 0 else 1

            if this.Inputs.ContainsKey(idx) then
                let (vIdx, vNode) = this.Inputs.Item(idx);
                if vNode.Outputs.ContainsKey(vIdx) then
                    this.Outputs <- this.Outputs.Add(0, vNode.Outputs.Item(vIdx))
                else 
                    this.Outputs <- this.Outputs.Add(0, Slot.emptyFromSlotType(slotType));
            else 
                this.Outputs <- this.Outputs.Add(0, Slot.emptyFromSlotType(slotType));
                

        member this.ExecuteInputs() =
            ()

        member this.PopupCB(option: string) =
            this.SlotType <- stringToSlotType(option);
            this.Title <- 
                match this.SlotType with
                    | SlotType.Number -> "Numeric Switch"
                    | SlotType.Text -> "Text Switch"
                    | _ ->  "Boolean Switch"




type NodeEditor() as _this =
    inherit GraphEdit()

    let mutable selected: List<GraphNode> = [];
    let mutable popupPosition: Vector2 = Vector2.Zero;

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


        if toNode.Inputs.ContainsKey(toIdx) then
            let (otherIdx, otherFromNode) = toNode.Inputs.Item(toIdx);
            this.FSDisconnect(otherFromNode.Name, otherIdx, toName, toIdx);
            //GD.Print("TO IDX: ", toIdx, toNode.Name, " FROM IDX: ", fromIdx, fromNode.Name, " OTHER IDX: ", otherIdx, otherFromNode.Name);
            if not(fromIdx = otherIdx && fromNode.Name = otherFromNode.Name) then
                this.ConnectNode(fromName, fromIdx, toName, toIdx) |> ignore
                toNode.Inputs <- toNode.Inputs.Add(toIdx, (fromIdx, fromNode))
        else
            this.ConnectNode(fromName, fromIdx, toName, toIdx) |> ignore
            toNode.Inputs <- toNode.Inputs.Add(toIdx, (fromIdx, fromNode))
        

    member this.FSDisconnect(fromName: string, fromIdx: int, toName: string, toIdx: int) =
        let toNode = this.GetNode<NodeGraphNode>(new NodePath(toName));
        let fromNode = this.GetNode<NodeGraphNode>(new NodePath(fromName));
        toNode.Inputs <- toNode.Inputs.Remove(toIdx)
        toNode.Outputs <- fromNode.Outputs.Remove(toIdx);
        this.DisconnectNode(fromName, fromIdx, toName, toIdx);



    member this.FSSelect(node: GraphNode) =
        selected <- (node :: selected);

    member this.FSUnselect(node: GraphNode) =
        // http://www.fssnip.net/1T/title/Remove-first-ocurrence-from-list
        let rec remove_first pred lst =
            match lst with
            | h::t when pred h -> t
            | h::t -> h::remove_first pred t
            | _ -> []
        selected <- selected |> remove_first (fun(n) -> node = n)

    member this.FSDelete() =
        for n in selected do
            n.Call("Close") |> ignore;

    member this.AddNode(node: NodeGraphNode, position: Vector2) =
        node.Offset <- this.ScrollOffset + popupPosition;
        this.AddChild(node);
        
    member this.PopupRequest(pos) =
        popupPosition <- pos

(*
    Actual Execution Engine
*)
[<AbstractClass>]
type NodeEngine() as _this =
    inherit Control()

    let frameCount: uint64 = 0UL;

    let mutable mouseInside = false;

    [<Export>]
    let nodePath: NodePath = new NodePath("GraphEdit");
    let editor = lazy(_this.GetNode<NodeEditor>(nodePath)); 
    let popup = lazy(_this.GetNode<PopupMenu>(new NodePath("GraphEditorPopup")));
    let playerInput = lazy(editor.Value.GetNode<PlayerControls.NodeGraphNodePlayerOutput>(new NodePath("PlayerInput")));
    let playerOutput = lazy(editor.Value.GetNode<PlayerControls.NodeGraphNodePlayerOutput>(new NodePath("PlayerOutput")));
    

    member this.PopupRequest(pos: Vector2) =
        let popup = popup.Value

        popup.SetPosition(pos);
        popup.Call("show_wrapper");

    override this._PhysicsProcess(delta: float32) =
        frameCount = frameCount + 1UL |> ignore

        if frameCount % 60UL % 10UL = 0UL then
            let po = playerOutput.Value;
            po.ExecuteInputs();
            po.Execute();

            let movementSpeed = Slot.percentage(po.GetMovementSpeed()) * -1f;
            let turningSpeed = Slot.percentage(po.GetTurningSpeed());
            let name = Slot.text(po.GetName());


            Player.Update(
                movementSpeed,
                turningSpeed,
                name
            );


