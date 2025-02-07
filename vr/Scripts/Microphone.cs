using Godot;
using System;
using Vosk;
using System.Text;

public partial class Microphone : Node
{
	private AudioEffectRecord _effect;
	private VoskRecognizer _recognizer;
	
	public override void _Ready()
	{
		// ตั้งค่า AudioEffectRecord
		int idx = AudioServer.GetBusIndex("Record");
		_effect = (AudioEffectRecord)AudioServer.GetBusEffect(idx, 0);

		// โหลดโมเดล Vosk (กำหนด path ของโมเดลที่ต้องการ)
		Model model = new Model("res://models/vosk-model-small-en-us");
		_recognizer = new VoskRecognizer(model, 16000);  // ใช้ sample rate 16kHz
	}

	private void OnRecordButtonPressed()
	{
		GD.Print("Record Button Pressed");

		if (_effect.IsRecordingActive())
		{
			_effect.SetRecordingActive(false);
			GetNode<Button>("RecordButton").Text = "Start Recording";
		}
		else
		{
			_effect.SetRecordingActive(true);
			GetNode<Button>("RecordButton").Text = "Stop Recording";
			ProcessAudioStream();
		}
	}

	private async void ProcessAudioStream()
	{
		while (_effect.IsRecordingActive())
		{
			// รับข้อมูลเสียงจาก AudioEffectRecord
			var audioData = _effect.GetRecording().Data;
			GD.Print("Audio Data Length: ", audioData.Length);
			// ส่งข้อมูลเสียงไปยัง Vosk โดยกำหนด sample rate เป็น 16000 Hz
			if (_recognizer.AcceptWaveform(audioData, 16000))
			{
				string result = _recognizer.Result();
				GD.Print("Recognized: " + result);

				// ตรวจสอบคำสั่งเสียง
				if (result.Contains("fire ball"))
				{
					TriggerSpell("fire ball");
				}
			}
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");  // Delay เล็กน้อย
		}
	}

	private void TriggerSpell(string spellName)
	{
		GD.Print($"Casting spell: {spellName}");

		// Load PackedScene and create an instance of the fireball
		var fireball = (Node3D)GD.Load<PackedScene>("res://scenes/FireBall.tscn").Instantiate();
		GetParent().AddChild(fireball);

		// กำหนดตำแหน่งเริ่มต้นของ fireball ให้ตรงกับตำแหน่งของผู้เล่น
		fireball.Translate(GetNode<Node3D>("Player").GlobalTransform.Origin);
	}


}
