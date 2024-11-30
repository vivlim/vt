using Terminal.Gui;

namespace vt.Ui;

public class PromptDialog(string message)
{
    public async Task<string?> Show()
    {
        await Task.CompletedTask;
        var tcs = new TaskCompletionSource<string?>();

        Application.MainLoop.Invoke(() =>
        {
            var d = new Dialog()
            {
                Width = 50,
                Height = 10,
                X = Pos.Center(),
                Y = Pos.Center(),
            };

            var l = new Label()
            {
                Text = message,
                X = 1,
                Y = 1,
            };
            d.Add(l);
            var field = new TextField()
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill(),
                Secret = true,
            };
            d.Add(field);

            var cancel = new Button()
            {
                Text = "cancel",
            };

            var confirm = new Button()
            {
                Text = "confirm",
                IsDefault = true,
            };
            d.AddButton(confirm);
            d.AddButton(cancel);

            confirm.Clicked += () =>
            {
                tcs.SetResult(field.Text.ToString());
                Application.RequestStop(d);
            };

            cancel.Clicked += () =>
            {
                tcs.SetResult(null);
                Application.RequestStop(d);
            };

            Application.Run(d);
        });

        return await tcs.Task;
    }

}
