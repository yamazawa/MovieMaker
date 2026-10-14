using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieMaker.Models;
using MovieMaker.Services;
using MovieMaker.Styles;

namespace MovieMaker.ViewModels;

/// <summary>
/// メイン画面のViewModel
///
/// 作成モード・再生モードの切り替え、配置済みオブジェクト一覧・選択状態、
/// 両モードで共有するタイムライン状態(現在時刻・全体時間)を持つ。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ClipboardImageService _clipboardImageService;
    private readonly ImageAssetService _imageAssetService;
    private readonly MovieProjectService _movieProjectService;
    private readonly DispatcherTimer _playbackTimer;
    private readonly Stack<MovieProject> _undoStack = new();
    private List<SceneObject>? _clipboard;

    // 復元中はユーザー操作ではないため、MarkDirty/Undoスナップショット記録を抑制するためのガード。
    private bool _isRestoring;
    private bool _isDirty;
    private DateTime _lastPlaybackTick;

    public MainViewModel(ClipboardImageService clipboardImageService, ImageAssetService imageAssetService, MovieProjectService movieProjectService)
    {
        _clipboardImageService = clipboardImageService;
        _imageAssetService = imageAssetService;
        _movieProjectService = movieProjectService;
        Objects.CollectionChanged += OnObjectsChanged;

        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LayoutConstants.PlaybackFrameIntervalMilliseconds) };
        _playbackTimer.Tick += OnPlaybackTick;

        LoadProject();
    }

    /// <summary>
    /// 変更があれば動画プロジェクトを保存する(自動保存タイマーから呼ばれる)
    /// </summary>
    public void SaveIfDirty()
    {
        if (!_isDirty)
            return;

        _movieProjectService.Save(CaptureSnapshot());
        _isDirty = false;
    }

    private void LoadProject()
    {
        var project = _movieProjectService.TryLoad();
        if (project is null)
            return;

        _isRestoring = true;
        Duration = project.Duration;
        BackgroundColorHex = project.BackgroundColorHex;
        TimelinePixelsPerSecond = project.TimelinePixelsPerSecond > 0 ? project.TimelinePixelsPerSecond : LayoutConstants.TimelinePixelsPerSecond;
        TimelineRowHeight = project.TimelineRowHeight > 0 ? project.TimelineRowHeight : LayoutConstants.TimelineRowHeight;
        TimelinePanelHeight = project.TimelinePanelHeight > 0 ? project.TimelinePanelHeight : LayoutConstants.TimelineHeight;
        WindowWidth = project.WindowWidth > 0 ? project.WindowWidth : LayoutConstants.WindowWidth;
        WindowHeight = project.WindowHeight > 0 ? project.WindowHeight : LayoutConstants.WindowHeight;

        foreach (var model in project.Objects)
            Objects.Add(SceneObjectViewModel.FromModel(model, _imageAssetService));

        CurrentTime = Math.Clamp(project.CurrentTime, 0, Duration);
        if (project.SelectedObjectId is { } selectedId && Objects.FirstOrDefault(obj => obj.Id == selectedId) is { } selected)
            SetSelectedObject(selected);

        _isRestoring = false;

        RefreshVisibleObjects();
    }

    private void MarkDirty()
    {
        if (!_isRestoring)
            _isDirty = true;
    }

    private MovieProject CaptureSnapshot() =>
        new(Duration, BackgroundColorHex, TimelinePixelsPerSecond, TimelineRowHeight, TimelinePanelHeight,
            CurrentTime, SelectedObject?.Id, WindowWidth, WindowHeight,
            Objects.Select(obj => obj.ToModel()).ToList());

    /// <summary>
    /// 編集操作の直前に呼ぶ(各ドラッグ系ビヘイビアのMouseLeftButtonDown、TextBoxのGotFocus、各編集コマンドの先頭から)。
    /// その時点の状態をUndoスタックへ積む。
    /// </summary>
    public void BeginUndoableChange()
    {
        if (_isRestoring)
            return;

        _undoStack.Push(CaptureSnapshot());
        UndoCommand.NotifyCanExecuteChanged();
    }

    private bool CanUndo() => _undoStack.Count > 0;

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoStack.Count == 0)
            return;

        ApplySnapshot(_undoStack.Pop());
        UndoCommand.NotifyCanExecuteChanged();
    }

    // Undoでは表示設定(TimelinePixelsPerSecond/TimelineRowHeight)は書き戻さない。編集操作の取り消し対象ではないため。
    private void ApplySnapshot(MovieProject snapshot)
    {
        _isRestoring = true;

        SelectedObject = null;
        AlignBaseObject = null;
        AlignTargetCount = 0;

        Objects.Clear();
        foreach (var model in snapshot.Objects)
            Objects.Add(SceneObjectViewModel.FromModel(model, _imageAssetService));

        Duration = snapshot.Duration;
        BackgroundColorHex = snapshot.BackgroundColorHex;

        _isRestoring = false;

        RefreshVisibleObjects();
        MarkDirty();
    }

    [ObservableProperty]
    private AppMode _mode = AppMode.Create;

    [ObservableProperty]
    private double _currentTime;

    [ObservableProperty]
    private double _duration = LayoutConstants.DefaultDurationSeconds;

    [ObservableProperty]
    private string _backgroundColorHex = LayoutConstants.DefaultBackgroundColorHex;

    // タイムラインの拡縮率(横:1秒あたりpx数、縦:1行の高さ)。
    [ObservableProperty]
    private double _timelinePixelsPerSecond = LayoutConstants.TimelinePixelsPerSecond;

    [ObservableProperty]
    private double _timelineRowHeight = LayoutConstants.TimelineRowHeight;

    // 中央(パレット+キャンバス)とタイムラインの境界(GridSplitterの位置)。次回起動時に復元する。
    [ObservableProperty]
    private double _timelinePanelHeight = LayoutConstants.TimelineHeight;

    // ウィンドウサイズ。次回起動時に復元する。
    [ObservableProperty]
    private double _windowWidth = LayoutConstants.WindowWidth;

    [ObservableProperty]
    private double _windowHeight = LayoutConstants.WindowHeight;

    [ObservableProperty]
    private SceneObjectViewModel? _selectedObject;

    // サイズを揃える機能の基準オブジェクトと、対象として選択中のオブジェクト数(パレット表示用)。
    [ObservableProperty]
    private SceneObjectViewModel? _alignBaseObject;

    [ObservableProperty]
    private int _alignTargetCount;

    // 時間の挿入・削除で使う秒数(現在時刻を起点とする)。
    [ObservableProperty]
    private double _timeAdjustSeconds = 1.0;

    public ObservableCollection<SceneObjectViewModel> Objects { get; } = [];

    // 現在時刻に表示範囲が重なるオブジェクトのみ。キャンバスの表示にバインドする。
    public ObservableCollection<SceneObjectViewModel> VisibleObjects { get; } = [];

    // 再生モード中の再生/一時停止状態。モード切替で再生モードに入るたびに再生中へ戻す。
    [ObservableProperty]
    private bool _isPlaying = true;

    [RelayCommand]
    private void ToggleMode() => Mode = Mode == AppMode.Create ? AppMode.Play : AppMode.Create;

    // モード切替時、現在時刻は引き継ぐ(作成モードの再生位置から再生開始し、作成モードに戻った時もその時刻のまま)。
    partial void OnModeChanged(AppMode value)
    {
        if (value == AppMode.Play)
        {
            IsPlaying = true;
            _lastPlaybackTick = DateTime.UtcNow;
            _playbackTimer.Start();
        }
        else
        {
            _playbackTimer.Stop();
        }
    }

    [RelayCommand]
    private void TogglePlayback()
    {
        IsPlaying = !IsPlaying;

        if (IsPlaying)
        {
            _lastPlaybackTick = DateTime.UtcNow;
            _playbackTimer.Start();
        }
        else
        {
            _playbackTimer.Stop();
        }
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (Duration <= 0)
            return;

        var now = DateTime.UtcNow;
        var elapsed = (now - _lastPlaybackTick).TotalSeconds;
        _lastPlaybackTick = now;

        var next = CurrentTime + elapsed;
        CurrentTime = next >= Duration ? next % Duration : next;
    }

    [RelayCommand]
    private void AddText()
    {
        BeginUndoableChange();

        var (x, y, width, height) = ResolveNewObjectBounds(LayoutConstants.DefaultObjectWidth, LayoutConstants.DefaultObjectHeight);

        var sceneObject = new SceneObjectViewModel(
            SceneObjectType.Text,
            x: x,
            y: y,
            width: width,
            height: height,
            startTime: CurrentTime,
            endTime: CurrentTime + LayoutConstants.DefaultObjectDurationSeconds)
        {
            FontSize = LayoutConstants.DefaultFontSize,
        };

        Objects.Add(sceneObject);
        SetSelectedObject(sceneObject);
    }

    [RelayCommand]
    private void AddImageFromClipboard()
    {
        if (!_clipboardImageService.TryGetImage(out var image) || image is null)
            return;

        AddImageObject(image);
    }

    [RelayCommand]
    private void AddImageFromFile(string filePath)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(filePath);
            image.EndInit();
            image.Freeze();

            AddImageObject(image);
        }
        catch (Exception ex) when (ex is NotSupportedException or FileFormatException or IOException)
        {
            // 画像として読み込めないファイルがドロップされた場合は何もしない。
        }
    }

    private void AddImageObject(BitmapSource image)
    {
        BeginUndoableChange();

        var maxWidth = LayoutConstants.CanvasWidth * 0.6;
        var maxHeight = LayoutConstants.CanvasHeight * 0.6;
        var scale = Math.Min(1.0, Math.Min(maxWidth / image.PixelWidth, maxHeight / image.PixelHeight));
        var (x, y, width, height) = ResolveNewObjectBounds(image.PixelWidth * scale, image.PixelHeight * scale);

        var sceneObject = new SceneObjectViewModel(
            SceneObjectType.Image,
            x: x,
            y: y,
            width: width,
            height: height,
            startTime: CurrentTime,
            endTime: CurrentTime + LayoutConstants.DefaultObjectDurationSeconds)
        {
            ImageSource = image,
            ImageFileName = _imageAssetService.SaveImage(image),
        };

        Objects.Add(sceneObject);
        SetSelectedObject(sceneObject);
    }

    // 新規オブジェクトの配置位置・サイズ。選択中オブジェクトがあればそれと同じ位置・サイズに合わせ、
    // なければキャンバス中央に既定サイズ(引数)で配置する。
    private (double X, double Y, double Width, double Height) ResolveNewObjectBounds(double defaultWidth, double defaultHeight)
    {
        if (SelectedObject is not null)
            return (SelectedObject.X, SelectedObject.Y, SelectedObject.Width, SelectedObject.Height);

        return ((LayoutConstants.CanvasWidth - defaultWidth) / 2, (LayoutConstants.CanvasHeight - defaultHeight) / 2, defaultWidth, defaultHeight);
    }

    // クリック選択の共通処理。サイズ揃えの基準選択中(AlignBaseObjectがある間)はSelectObjectCommand側で分岐するため、
    // 新規追加時の自動選択など、通常の単一選択が必要な箇所はこちらを直接呼ぶ。
    private void SetSelectedObject(SceneObjectViewModel sceneObject)
    {
        foreach (var obj in Objects)
            obj.IsSelected = obj == sceneObject;

        SelectedObject = sceneObject;
    }

    // キャンバス/タイムラインのブロッククリック共通処理。
    // サイズ揃えの基準(AlignBaseObject)を選択中は、クリックしたブロックを対象(IsAlignTarget)に追加/除外する。
    // タイムライン側のクリックの場合、現在時刻が既にそのオブジェクトの表示区間内に無ければ、表示開始時刻(左端)へ移動する。
    private void HandleObjectClick(SceneObjectViewModel sceneObject, bool isTimelineClick)
    {
        if (AlignBaseObject is not null)
        {
            if (sceneObject != AlignBaseObject)
                sceneObject.IsAlignTarget = !sceneObject.IsAlignTarget;

            return;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            ToggleSelection(sceneObject);
        else
            SetSelectedObject(sceneObject);

        if (!isTimelineClick)
            return;

        var isCurrentTimeInRange = CurrentTime >= sceneObject.StartTime && CurrentTime < sceneObject.EndTime;
        if (!isCurrentTimeInRange)
            CurrentTime = Math.Clamp(sceneObject.StartTime, 0, Duration);
    }

    // Ctrl+クリックでの複数選択。選択中(IsSelected)を解除せず対象のみ反転する。
    // パレットの編集項目は単一オブジェクト向けのため、SelectedObjectは直近で選択状態になったものを指す。
    private void ToggleSelection(SceneObjectViewModel sceneObject)
    {
        sceneObject.IsSelected = !sceneObject.IsSelected;
        SelectedObject = sceneObject.IsSelected ? sceneObject : Objects.FirstOrDefault(obj => obj.IsSelected);
    }

    [RelayCommand]
    private void SelectObject(SceneObjectViewModel sceneObject) => HandleObjectClick(sceneObject, isTimelineClick: false);

    [RelayCommand]
    private void SelectTimelineObject(SceneObjectViewModel sceneObject) => HandleObjectClick(sceneObject, isTimelineClick: true);

    // 選択中(IsSelected)の全オブジェクトをコピーする。複数選択中でも対応するため、単一選択のSelectedObjectではなくIsSelectedで集める。
    [RelayCommand]
    private void Copy()
    {
        var selected = Objects.Where(obj => obj.IsSelected).ToList();
        if (selected.Count == 0)
            return;

        _clipboard = selected.Select(obj => obj.ToModel()).ToList();
    }

    // 現在時刻を基準に、コピー時の相対的な時刻関係(開始時刻の差・表示時間)を保ったまま貼り付ける。
    [RelayCommand]
    private void Paste()
    {
        if (_clipboard is null || _clipboard.Count == 0)
            return;

        BeginUndoableChange();

        var anchorTime = _clipboard.Min(model => model.StartTime);
        var pasted = _clipboard
            .Select(model =>
            {
                var newStart = CurrentTime + (model.StartTime - anchorTime);
                return model with { Id = Guid.NewGuid(), StartTime = newStart, EndTime = newStart + (model.EndTime - model.StartTime) };
            })
            .Select(model => SceneObjectViewModel.FromModel(model, _imageAssetService))
            .ToList();

        foreach (var obj in pasted)
            Objects.Add(obj);

        foreach (var obj in Objects)
            obj.IsSelected = pasted.Contains(obj);

        SelectedObject = pasted.FirstOrDefault();
    }

    // タイムチャートを①開始時刻の昇順、②開始時刻が同じ場合は終了時刻の降順、で並べ替える。
    [RelayCommand]
    private void SortObjects()
    {
        BeginUndoableChange();

        var sorted = Objects.OrderBy(obj => obj.StartTime).ThenByDescending(obj => obj.EndTime).ToList();

        for (var i = 0; i < sorted.Count; i++)
        {
            var currentIndex = Objects.IndexOf(sorted[i]);
            if (currentIndex != i)
                Objects.Move(currentIndex, i);
        }
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedObject is null)
            return;

        BeginUndoableChange();

        Objects.Remove(SelectedObject);
        SelectedObject = null;
    }

    // キャンバス基準の位置揃え。選択中(IsSelected)の全オブジェクトに適用する(複数選択時は各オブジェクトが個別にキャンバス端・中央へ揃う)。
    // 「サイズを揃える」(基準オブジェクトに合わせる機能)とは別の機能。
    [RelayCommand]
    private void AlignLeft() => AlignSelectedPosition(obj => obj.X = 0);

    [RelayCommand]
    private void AlignCenterX() => AlignSelectedPosition(obj => obj.X = (LayoutConstants.CanvasWidth - obj.Width) / 2);

    [RelayCommand]
    private void AlignRight() => AlignSelectedPosition(obj => obj.X = LayoutConstants.CanvasWidth - obj.Width);

    [RelayCommand]
    private void AlignTop() => AlignSelectedPosition(obj => obj.Y = 0);

    [RelayCommand]
    private void AlignCenterY() => AlignSelectedPosition(obj => obj.Y = (LayoutConstants.CanvasHeight - obj.Height) / 2);

    [RelayCommand]
    private void AlignBottom() => AlignSelectedPosition(obj => obj.Y = LayoutConstants.CanvasHeight - obj.Height);

    private void AlignSelectedPosition(Action<SceneObjectViewModel> align)
    {
        var selected = Objects.Where(obj => obj.IsSelected).ToList();
        if (selected.Count == 0)
            return;

        BeginUndoableChange();

        foreach (var obj in selected)
            align(obj);
    }

    [RelayCommand]
    private void SetAlignBase()
    {
        if (SelectedObject is null)
            return;

        AlignBaseObject = SelectedObject;
    }

    [RelayCommand]
    private void ConfirmAlignSizes()
    {
        if (AlignBaseObject is not { } baseObject)
            return;

        BeginUndoableChange();

        foreach (var obj in Objects.Where(obj => obj.IsAlignTarget && obj != baseObject))
        {
            obj.X = baseObject.X;
            obj.Y = baseObject.Y;
            obj.Width = baseObject.Width;
            obj.Height = baseObject.Height;
            obj.IsAlignTarget = false;
        }

        AlignBaseObject = null;
    }

    [RelayCommand]
    private void CancelAlign()
    {
        foreach (var obj in Objects.Where(obj => obj.IsAlignTarget))
            obj.IsAlignTarget = false;

        AlignBaseObject = null;
    }

    // 現在時刻の位置にTimeAdjustSeconds分の時間を挿入する(引き延ばし)。
    // 現在時刻を跨ぐオブジェクト(開始<=現在時刻<終了)は終了時刻が伸び、
    // 現在時刻より後ろにある(開始>現在時刻)オブジェクトは丸ごと後ろへずれる。
    [RelayCommand]
    private void InsertTime()
    {
        if (TimeAdjustSeconds <= 0)
            return;

        BeginUndoableChange();

        foreach (var obj in Objects)
        {
            // Start側は変化しないか現在値のままのため、先にEnd側(広がる側)を反映してからStartを反映する。
            obj.EndTime = ShiftForInsert(obj.EndTime, CurrentTime, TimeAdjustSeconds);
            obj.StartTime = ShiftForInsert(obj.StartTime, CurrentTime, TimeAdjustSeconds);
        }

        Duration += TimeAdjustSeconds;
    }

    private static double ShiftForInsert(double time, double insertAt, double insertSeconds) =>
        time <= insertAt ? time : time + insertSeconds;

    // 現在時刻からTimeAdjustSeconds分の区間を削除する(挿入の逆)。
    // 区間に完全に含まれるオブジェクトは削除し、区間と重なるオブジェクトは重なった分だけ短くする。
    [RelayCommand]
    private void DeleteTimeRange()
    {
        if (TimeAdjustSeconds <= 0)
            return;

        BeginUndoableChange();

        var deleteStart = CurrentTime;
        var deleteEnd = CurrentTime + TimeAdjustSeconds;

        for (var i = Objects.Count - 1; i >= 0; i--)
        {
            var obj = Objects[i];
            var newStart = ShiftForDelete(obj.StartTime, deleteStart, deleteEnd);
            var newEnd = ShiftForDelete(obj.EndTime, deleteStart, deleteEnd);

            if (newEnd <= newStart)
            {
                Objects.RemoveAt(i);
                continue;
            }

            // Start/End共に縮む(か変化しない)方向のため、先にStart側を反映してからEnd側を反映する。
            obj.StartTime = newStart;
            obj.EndTime = newEnd;
        }

        if (SelectedObject is not null && !Objects.Contains(SelectedObject))
            SelectedObject = null;

        if (AlignBaseObject is not null && !Objects.Contains(AlignBaseObject))
            AlignBaseObject = null;

        Duration = Math.Max(LayoutConstants.MinDurationSeconds, Duration - TimeAdjustSeconds);
    }

    private static double ShiftForDelete(double time, double deleteStart, double deleteEnd) =>
        time < deleteStart ? time : (time >= deleteEnd ? time - (deleteEnd - deleteStart) : deleteStart);

    partial void OnCurrentTimeChanged(double value)
    {
        RefreshVisibleObjects();
        MarkDirty();
    }

    partial void OnSelectedObjectChanged(SceneObjectViewModel? value) => MarkDirty();

    // ウィンドウサイズ・パレット/タイムライン境界は最小値を下回らないようクランプする。
    partial void OnWindowWidthChanged(double value)
    {
        var clamped = Math.Max(LayoutConstants.WindowMinWidth, value);
        if (clamped != value)
        {
            WindowWidth = clamped;
            return;
        }

        MarkDirty();
    }

    partial void OnWindowHeightChanged(double value)
    {
        var clamped = Math.Max(LayoutConstants.WindowMinHeight, value);
        if (clamped != value)
        {
            WindowHeight = clamped;
            return;
        }

        MarkDirty();
    }

    partial void OnTimelinePanelHeightChanged(double value)
    {
        var clamped = Math.Max(LayoutConstants.TimelineMinHeight, value);
        if (clamped != value)
        {
            TimelinePanelHeight = clamped;
            return;
        }

        MarkDirty();
    }

    partial void OnTimeAdjustSecondsChanged(double value)
    {
        var clamped = Math.Max(LayoutConstants.TimeStepSeconds, Math.Round(value, 1));
        if (clamped != value)
            TimeAdjustSeconds = clamped;
    }

    // 動画の長さは1秒～10分の範囲にクランプする。
    partial void OnDurationChanged(double value)
    {
        var clamped = Math.Clamp(Math.Round(value, 1), LayoutConstants.MinDurationSeconds, LayoutConstants.MaxDurationSeconds);
        if (clamped != value)
        {
            Duration = clamped;
            return;
        }

        MarkDirty();
    }

    partial void OnTimelinePixelsPerSecondChanged(double value)
    {
        var clamped = Math.Clamp(value, LayoutConstants.TimelineZoomXMin, LayoutConstants.TimelineZoomXMax);
        if (clamped != value)
            TimelinePixelsPerSecond = clamped;
    }

    partial void OnTimelineRowHeightChanged(double value)
    {
        var clamped = Math.Clamp(value, LayoutConstants.TimelineZoomYMin, LayoutConstants.TimelineZoomYMax);
        if (clamped != value)
            TimelineRowHeight = clamped;
    }

    private void OnObjectsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SceneObjectViewModel item in e.OldItems)
                item.PropertyChanged -= OnSceneObjectPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (SceneObjectViewModel item in e.NewItems)
                item.PropertyChanged += OnSceneObjectPropertyChanged;
        }

        RefreshVisibleObjects();
        MarkDirty();
    }

    private void OnSceneObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SceneObjectViewModel.StartTime) or nameof(SceneObjectViewModel.EndTime))
            RefreshVisibleObjects();

        if (e.PropertyName == nameof(SceneObjectViewModel.IsAlignTarget))
            AlignTargetCount = Objects.Count(obj => obj.IsAlignTarget);

        MarkDirty();
    }

    private void RefreshVisibleObjects()
    {
        var visible = Objects.Where(obj => obj.StartTime <= CurrentTime && CurrentTime < obj.EndTime).ToList();

        for (var i = VisibleObjects.Count - 1; i >= 0; i--)
        {
            if (!visible.Contains(VisibleObjects[i]))
                VisibleObjects.RemoveAt(i);
        }

        foreach (var obj in visible)
        {
            if (!VisibleObjects.Contains(obj))
                VisibleObjects.Add(obj);
        }
    }
}
