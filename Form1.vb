Imports System.Net.Sockets
Imports System.IO
Imports Newtonsoft.Json
Imports System.ComponentModel

Public Class Form1
    '输入框内回车使焦点向后一位
    Private Sub TextBox1_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = ChrW(13) Then
            SendKeys.Send("{TAB}")
        End If
    End Sub

    '输入框内回车使焦点向后一位
    Private Sub TextBox2_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox2.KeyPress
        If e.KeyChar = ChrW(13) Then
            SendKeys.Send("{TAB}")
        End If
    End Sub

    '按下登录按钮后的操作
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        '以下是判断输入框状态的操作，并对为空的输入框进行提示
        If TextBox1.Text = "" And TextBox2.Text = "" Then
            MsgBox("请输入用户名和密码", vbExclamation, "警告")
        ElseIf TextBox1.Text = "" Then
            MsgBox("请输入用户名后再进行登录", vbExclamation, "警告")
        ElseIf TextBox2.Text = "" Then
            MsgBox("请输入密码后再进行登录", vbExclamation, "警告")
        ElseIf ComboBox1.SelectedIndex = -1 Then
            MsgBox("请选择登录域", vbExclamation, "警告")
        Else
            '以下是对表单数据进行转换操作
            Dim domain As String
            Dim s As String
            Dim a As Byte()
            Dim username As String = TextBox1.Text
            Dim password As String

            domain = ComboBox1.SelectedItem

            '由于校园网认证域的 free-student 在数据包里没有分隔符，需要进行文本替换
            If ComboBox1.SelectedItem = "free-student" Then
                domain = "freestudent"
            End If

            a = System.Text.Encoding.Default.GetBytes(TextBox2.Text)    '读取密码输入框的内容并赋值给 a
            s = Convert.ToBase64String(a)                               '将 a 存取的密码明文转换为 Base64 编码并赋值给 s
            password = Uri.EscapeDataString(s)                          '对 s 存取的密码进行 URL 编码，使之可以在数据包中传输

            '以下是向指定服务器发送登录表单的操作
            Dim login_site As String = "http://[Your Auth Server Here]/index.php/index/login"      '接收 POST 请求的服务器地址
            Dim login_POST As String = "username=" + username + "&domain=" + domain + "&password=" + password + "&enablemacauth=0"

            Dim login_req As Net.HttpWebRequest
            Dim login_resp As Net.HttpWebResponse

            login_req = CType(Net.WebRequest.Create(login_site), Net.HttpWebRequest)
            login_req.UserAgent = "Login UniNET 0.1.15"
            login_req.AllowAutoRedirect = True
            login_req.ContentType = "application/x-www-form-urlencoded"
            login_req.ContentLength = login_POST.Length
            login_req.Method = "POST"
            login_req.KeepAlive = True

            Dim login_requestStream As IO.Stream = login_req.GetRequestStream()
            Dim login_postBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(login_POST)
            login_requestStream.Write(login_postBytes, 0, login_postBytes.Length)
            login_requestStream.Close()

            login_resp = CType(login_req.GetResponse(), Net.HttpWebResponse)

            '获取详细登录信息
            Dim state_site As String = "http://[Your Auth Server Here]/index.php/index/init"
            Dim state_POST As String = "Check state"

            Dim state_req As Net.HttpWebRequest
            Dim state_resp As Net.HttpWebResponse

            state_req = CType(Net.WebRequest.Create(state_site), Net.HttpWebRequest)
            state_req.UserAgent = "Login UniNET 0.1.15"
            state_req.AllowAutoRedirect = True
            state_req.ContentType = "application/x-www-form-urlencoded"
            state_req.ContentLength = state_POST.Length
            state_req.Method = "POST"
            state_req.KeepAlive = True

            Dim state_requestStream As IO.Stream = state_req.GetRequestStream()
            Dim state_postBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(state_POST)
            state_requestStream.Write(state_postBytes, 0, state_postBytes.Length)
            state_requestStream.Close()

            state_resp = CType(state_req.GetResponse(), Net.HttpWebResponse)

            '获取登录结果操作
            Dim login_dataStream As Stream = login_resp.GetResponseStream()
            Dim login_reader As New StreamReader(login_dataStream)
            Dim login_responseFromServer As String = login_reader.ReadToEnd()
            Dim login_JsonStr As String = login_responseFromServer
            Dim login_js As Object = New System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(Of Object)(login_JsonStr)
            Dim status = login_js("status")
            Dim login_status = login_js("status")
            Dim login_info = login_js("info")

            Dim msg = (login_info)
            Dim caption = ("登录错误")
            If login_status = 0 Then
                MessageBox.Show(msg, caption, buttons:=vbOKOnly, icon:=vbCritical)
            End If

            '获取详细登录信息操作
            Dim state_dataStream As Stream = state_resp.GetResponseStream()
            Dim state_reader As New StreamReader(state_dataStream)
            Dim state_responseFromServer As String = state_reader.ReadToEnd()
            Dim state_JsonStr As String = state_responseFromServer
            Dim state_js As Object = New System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(Of Object)(state_JsonStr)

            If status = 0 Then
                TextBox1.Enabled = True
                TextBox2.Enabled = True
                ComboBox1.Enabled = True
                CheckBox1.Enabled = True
                Me.Size = New Size(670, 300)
                Label5.Hide()
                Label6.Hide()
                Label7.Hide()
                Label8.Hide()
                Label9.Hide()
                Label9.Text = ("未知")
                Label10.Hide()
                Label10.Text = ("未知")
                Label11.Hide()
                Label11.Text = ("未知")
                Button2.Hide()
                Button1.Enabled = True
            End If

            If status = 1 Then
                TextBox1.Enabled = False
                TextBox2.Enabled = False
                ComboBox1.Enabled = False
                CheckBox1.Enabled = False
                Me.Size = New Size(670, 470)
                Dim state_username = state_js("logout_username")
                Dim state_domain = state_js("logout_domain")
                Dim state_ip = state_js("logout_ip")

                Line1.Show()
                Label5.Show()
                Label6.Show()
                Label7.Show()
                Label8.Show()
                Label9.Show()
                Label9.Text = (state_username)
                Label10.Show()
                Label10.Text = (state_domain)
                Label11.Show()
                Label11.Text = (state_ip)
                Button2.Show()
                Button1.Enabled = False
                ToolStripStatusLabel2.Text = "已登录"
                ToolStripStatusLabel2.ForeColor = Color.Green
                ToolStripStatusLabel3.ForeColor = Color.Green

                '登录成功后，默认记住用户名和认证域，通过 checkbox1 的状态决定是否记住密码
                My.Settings.username_text = TextBox1.Text
                Dim username_text As String = My.Settings.username_text
                My.Settings.chkbox_stat = CheckBox1.Checked
                Dim chkbox_stat As Boolean = My.Settings.chkbox_stat
                My.Settings.combox_index = ComboBox1.SelectedIndex
                Dim combobox_index = My.Settings.combox_index
                If CheckBox1.Checked = True Then
                    My.Settings.password_text = TextBox2.Text
                    Dim password_text As String = My.Settings.password_text
                End If
                My.Settings.Save()
            End If


            '结束读取连接
            login_reader.Close()
            state_reader.Close()
            login_dataStream.Close()
            state_dataStream.Close()
            login_resp.Close()
            state_resp.Close()
        End If

    End Sub

    '点击退出登录按钮的操作
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        '退出登录，通过直接访问 URL 实现
        Dim cweb As String = "http://[Your Auth Server Here]/index.php/index/logout"
        Dim POST As String = "logout"

        Dim request As Net.HttpWebRequest
        Dim response As Net.HttpWebResponse

        request = CType(Net.WebRequest.Create(cweb), Net.HttpWebRequest)
        request.UserAgent = "Login UniNET 0.1.15"
        request.AllowAutoRedirect = True
        request.ContentType = "application/x-www-form-urlencoded"
        request.ContentLength = POST.Length
        request.Method = "POST"
        request.KeepAlive = True

        Dim requestStream As IO.Stream = request.GetRequestStream()
        Dim postBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(POST)
        requestStream.Write(postBytes, 0, postBytes.Length)
        requestStream.Close()

        response = CType(request.GetResponse(), Net.HttpWebResponse)
        response.Close()

        '未登录界面
        TextBox1.Enabled = True
        TextBox2.Enabled = True
        ComboBox1.Enabled = True
        CheckBox1.Enabled = True
        Me.Size = New Size(670, 300)
        Line1.Hide()
        Label5.Hide()
        Label6.Hide()
        Label7.Hide()
        Label8.Hide()
        Label9.Hide()
        Label10.Hide()
        Label11.Hide()
        Button1.Enabled = True
        Button2.Hide()
        ToolStripStatusLabel2.Text = "已连接到校园网，等待登录"
        ToolStripStatusLabel2.ForeColor = Color.Black
        ToolStripStatusLabel3.ForeColor = Color.Black
    End Sub

    '程序启动操作
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '初始启动界面
        TextBox1.Enabled = True
        TextBox2.Enabled = True
        ComboBox1.Enabled = True
        CheckBox1.Enabled = True
        Me.Size = New Size(670, 300)
        Line1.Hide()
        Label5.Hide()
        Label6.Hide()
        Label7.Hide()
        Label8.Hide()
        Label9.Hide()
        Label9.Text = ("未知")
        Label10.Hide()
        Label10.Text = ("未知")
        Label11.Hide()
        Label11.Text = ("未知")
        Button2.Hide()
        Button1.Enabled = True

        '检测网络状态
        Dim text As String
        Dim caption As String
        If My.Computer.Network.IsAvailable = True Then
            '对认证服务器进行一次 Ping
            If My.Computer.Network.Ping("[Your Auth Server Here]") Then
                ToolStripStatusLabel2.Text = "已连接到校园网，等待登录"

                '读取上一次已记住的登录信息
                CheckBox1.Checked = My.Settings.chkbox_stat
                ComboBox1.SelectedIndex = My.Settings.combox_index
                If CheckBox1.Checked = True Then
                    TextBox1.Text = My.Settings.username_text
                    TextBox2.Text = My.Settings.password_text
                Else
                    CheckBox1.Checked = False
                    TextBox1.Text = My.Settings.username_text
                    TextBox2.Text = ""
                End If

                '读取详细登录信息
                Dim state_site As String = "http://[Your Auth Server Here]/index.php/index/init"
                    Dim POST As String = "Check state"

                    Dim state_req As Net.HttpWebRequest
                    Dim state_resp As Net.HttpWebResponse

                    state_req = CType(Net.WebRequest.Create(state_site), Net.HttpWebRequest)
                    state_req.UserAgent = "Login UniNET 0.1.15"
                    state_req.AllowAutoRedirect = True
                    state_req.ContentType = "application/x-www-form-urlencoded"
                    state_req.ContentLength = POST.Length
                    state_req.Method = "POST"
                    state_req.KeepAlive = True

                    Dim state_requestStream As IO.Stream = state_req.GetRequestStream()
                    Dim state_postBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(POST)
                    state_requestStream.Write(state_postBytes, 0, state_postBytes.Length)
                    state_requestStream.Close()

                    state_resp = CType(state_req.GetResponse(), Net.HttpWebResponse)


                    '获取详细登录信息操作
                    Dim state_dataStream As Stream = state_resp.GetResponseStream()
                    Dim state_reader As New StreamReader(state_dataStream)
                    Dim state_responseFromServer As String = state_reader.ReadToEnd()
                    Dim state_JsonStr As String = state_responseFromServer
                    Dim state_js As Object = New System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(Of Object)(state_JsonStr)
                    Dim status = state_js("status")

                    If status = 0 Then
                    TextBox1.Enabled = True
                    TextBox2.Enabled = True
                    ComboBox1.Enabled = True
                    CheckBox1.Enabled = True
                    Me.Size = New Size(670, 300)
                    Line1.Hide()
                    Label5.Hide()
                    Label6.Hide()
                    Label7.Hide()
                    Label8.Hide()
                    Label9.Hide()
                    Label9.Text = ("未知")
                    Label10.Hide()
                    Label10.Text = ("未知")
                    Label11.Hide()
                    Label11.Text = ("未知")
                    Button2.Hide()
                    Button1.Enabled = True
                    ToolStripStatusLabel2.Text = "已连接到校园网，等待登录"

                End If

                If status = 1 Then
                    Me.Size = New Size(670, 470)
                    Dim state_username = state_js("logout_username")
                    Dim state_domain = state_js("logout_domain")
                    Dim state_ip = state_js("logout_ip")

                    TextBox1.Enabled = False
                    TextBox2.Enabled = False
                    ComboBox1.Enabled = False
                    CheckBox1.Enabled = False
                    Line1.Show()
                    Label5.Show()
                    Label6.Show()
                    Label7.Show()
                    Label8.Show()
                    Label9.Show()
                    Label9.Text = (state_username)
                    Label10.Show()
                    Label10.Text = (state_domain)
                    Label11.Show()
                    Label11.Text = (state_ip)
                    Button2.Show()
                    Button1.Enabled = False
                    ToolStripStatusLabel2.Text = "已登录"
                    ToolStripStatusLabel2.ForeColor = Color.Green
                    ToolStripStatusLabel3.ForeColor = Color.Green
                End If
            Else
                text = ("未检测到校园网环境" & Chr(13) & "请连接到校园网后重试")
                caption = "错误"
                Dim selection As DialogResult = MessageBox.Show(text, caption, buttons:=vbOKOnly, icon:=vbCritical)

                If selection = DialogResult.OK Then
                    Me.Dispose()
                End If
                ToolStripStatusLabel2.Text = "连接超时，请检查网络设置"
                ToolStripStatusLabel2.ForeColor = Color.Red
                ToolStripStatusLabel3.ForeColor = Color.Red
                MsgBox("连接超时，请检查网络设置", vbExclamation, "警告")
            End If

        Else
            MsgBox("请检查网络设置", vbExclamation, "警告")
        End If

    End Sub

    '退出程序
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        '默认记住用户名和认证域，直接退出程序时不需要记住密码
        My.Settings.username_text = TextBox1.Text
        Dim username_text As String = My.Settings.username_text
        My.Settings.combox_index = ComboBox1.SelectedIndex
        Dim combobox_index = My.Settings.combox_index
        If ToolStripStatusLabel3.ForeColor = Color.Green Then
            My.Settings.chkbox_stat = CheckBox1.Checked
            Dim chkbox_stat As Boolean = My.Settings.chkbox_stat
        End If
        My.Settings.Save()

        Me.Dispose()
    End Sub

    '点击顶栏帮助按钮的操作
    Private Sub Form1_HelpButtonClicked(sender As Object, e As CancelEventArgs) Handles Me.HelpButtonClicked
        Dim text As String = ("        本程序可用于登录校园网                    " & Chr(13) & "        请在校园网环境下使用" & Chr(13) & "        账号问题请联系所在楼栋校园网维护人员" & Chr(13) & "" & Chr(13) & "        开发者：ymckc@foxmail.com" & Chr(13) & "        2022年10月")
        MessageBox.Show(text, caption:="关于本程序", buttons:=vbOKOnly, icon:=vbInformation)
    End Sub

    '打开自助服务网页
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        System.Diagnostics.Process.Start("http://[Your Auth Server Here]/service.php")
    End Sub

    '程序结束时操作
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        '默认记住用户名和认证域，直接退出程序时不需要记住密码
        My.Settings.username_text = TextBox1.Text
        Dim username_text As String = My.Settings.username_text
        My.Settings.combox_index = ComboBox1.SelectedIndex
        Dim combobox_index = My.Settings.combox_index
        If ToolStripStatusLabel3.ForeColor = Color.Green Then
            My.Settings.chkbox_stat = CheckBox1.Checked
            Dim chkbox_stat As Boolean = My.Settings.chkbox_stat
        End If
        My.Settings.Save()
    End Sub

End Class
