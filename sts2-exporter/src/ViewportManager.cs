using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace STS2Export;

public partial class ViewportManager : Node {
    private static ViewportManager _inst = new();
    private const int NumViewportsAvailable = 5;

    private VP[] viewports = new VP[NumViewportsAvailable];
    private static readonly Queue<DrawRequest> drawQueue = [];
    private int nextViewport = 0;
    private bool AllViewportsBusy() => nextViewport >= NumViewportsAvailable;

    public override void _Ready() {
        Performance.AddCustomMonitor("ViewportDrawer/num_viewports", Callable.From(() => nextViewport));
        _inst = this;
        for (int i = 0; i < viewports.Length; i++) {
            VP viewport = new();
            AddChild(viewport);
            viewports[i] = viewport;
        }
    }

    public static void AddToTree(SceneTree tree) {
        if (_inst.IsInsideTree()) return;
        tree.Root.AddChild(_inst);
    }

    public override void _Process(double delta) {
        nextViewport = 0;
        while (drawQueue.Count > 0 && TakeRequest());
    }

    private bool TakeRequest() {
        if (AllViewportsBusy() || viewports[0] == null) return false;
        while (viewports[nextViewport++].Doing)
            if (nextViewport >= NumViewportsAvailable)
                return false;
        viewports[nextViewport - 1].TakeRequest(drawQueue.Dequeue());
        return true;
    }

    //public static Task<Image> RequestDraw(Vector2 size, Action<VP.Drawer> drawMethod, Action<VP.Drawer> onStart = null) => RequestDraw((Vector2I)size, drawMethod, onStart);
    //public static Task<Image> RequestDraw(Vector2I size, Action<VP.Drawer> drawMethod, Action<VP.Drawer> onStart = null) => RequestDraw(new(size, drawMethod, onStart));
    public static Task<Image> RequestDraw(DrawRequest request) {
        drawQueue.Enqueue(request);
        _inst?.TakeRequest();
        return request.Task.Task;
    }

    //public static Task DrawAndSave(string path, Vector2 size, Action<VP.Drawer> drawMethod, Action<VP.Drawer> onStart = null) => DrawAndSave(path, (Vector2I)size, drawMethod, onStart);
    //public static async Task DrawAndSave(string path, Vector2I size, Action<VP.Drawer> drawMethod, Action<VP.Drawer> onStart = null) {
    //    var img = await RequestDraw(size, drawMethod, onStart);
    //    img.SavePng(path);
    //}

    public readonly struct DrawRequest(Vector2I dimensions, Action<VP.Drawer> action = null, Action<VP.Drawer> onStart = null) {
        public readonly TaskCompletionSource<Image> Task = new();
        public readonly Vector2I Dimensions = dimensions;
        public readonly Action<VP.Drawer> Action = action;
        public readonly Action<VP.Drawer> OnStart = onStart;
    }

    public partial class VP : SubViewport {
        private Drawer drawer = new();
        public bool Doing = false;

        public override void _Ready() {
            base._Ready();
            TransparentBg = true;
            AddChild(drawer);
            RenderTargetUpdateMode = UpdateMode.Disabled;
        }

        public async void TakeRequest(DrawRequest request) {
            Doing = true;
            if (Size != request.Dimensions)
                Size = request.Dimensions;
            RenderTargetUpdateMode = UpdateMode.Once;
            drawer.DrawAction = request.Action;
            drawer.ImageSize = request.Dimensions;
            request.OnStart?.Invoke(drawer);
            drawer.QueueRedraw();
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            var img = GetTexture().GetImage();
            request.Task.TrySetResult(img);
            Doing = false;
            foreach (Node n in drawer.GetChildren()) {
                drawer.RemoveChild(n);
                n.QueueFree();
            }
        }

        public partial class Drawer : Node2D {
            public Action<Drawer> DrawAction;
            public Vector2I ImageSize;

            public override void _Draw() {
                base._Draw();
                if (DrawAction == null) return;
                DrawSetTransform(Vector2.Zero, 0, Vector2.One);
                DrawAction(this);
            }
        }
    }
}