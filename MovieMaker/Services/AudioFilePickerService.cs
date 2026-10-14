using Microsoft.Win32;

namespace MovieMaker.Services;

/// <summary>
/// 音声ファイル(BGM/SE)選択用のファイルダイアログを開くサービス
/// </summary>
public class AudioFilePickerService
{
    public string? PickFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "音声ファイル (*.mp3;*.wav;*.wma;*.m4a)|*.mp3;*.wav;*.wma;*.m4a|すべてのファイル (*.*)|*.*",
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
