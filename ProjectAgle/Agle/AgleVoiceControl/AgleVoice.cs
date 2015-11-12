using Microsoft.Kinect;
using Microsoft.Samples.Kinect.SpeechBasics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Runtime.InteropServices;

namespace ProjectAgle.Agle.AgleVoiceControl
{
    class AgleVoice
    {
        private KinectSensor kinectSensor = null;
        private KinectAudioStream convertStream = null;
        private SpeechRecognitionEngine speechEngine = null;
        private Choices agleVoiceDictionary = null;
        internal event EventHandler<string> UpdateVoiceCommand;
        internal event EventHandler<AgleView> UpdateAgleViewByVoice;
        public AgleVoice()
        {
        }

        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected. 
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (COMException)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        internal bool TryInitializeAgleVoice(KinectSensor kinectSensor)
        {
            if (null == kinectSensor)
            {
                return false;
            }
            this.kinectSensor = kinectSensor;
            IReadOnlyList<AudioBeam> audioBeamList = this.kinectSensor.AudioSource.AudioBeams;
            System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();
            this.convertStream = new KinectAudioStream(audioStream);

            RecognizerInfo ri = TryGetKinectRecognizer();

            if (null != ri)
            {

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                this.agleVoiceDictionary = new Choices();
                this.CreateAgleVoiceDictionary();
                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(this.agleVoiceDictionary);

                var g = new Grammar(gb);
                this.speechEngine.LoadGrammar(g);
                this.speechEngine.SpeechRecognized += this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected += this.SpeechRejected;

                // let the convertStream know speech is going active
                this.convertStream.SpeechActive = true;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                this.speechEngine.SetInputToAudioStream(
                    this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                return false;
            }
            return true;
        }

        private void CreateAgleVoiceDictionary()
        {
            this.agleVoiceDictionary.Add(new SemanticResultValue("agle", "AGLE"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("color", "COLORFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("color frame", "COLORFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("color view", "COLORFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("kinect color", "COLORFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("depth", "DEPTHFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("depth frame", "DEPTHFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("depth view", "DEPTHFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("kinect depth", "DEPTHFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("infrared", "INFRAREDFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("infrared frame", "INFRAREDFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("infrared view", "INFRAREDFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("infrared depth", "INFRAREDFRAME"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("main camera", "MAINFORWARD"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("main", "MAINFORWARD"));
            this.agleVoiceDictionary.Add(new SemanticResultValue("forward", "MAINFORWARD"));

        }
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "AGLE":
                        UpdateVoiceCommand(this, "Agle");
                        break;

                    case "COLORFRAME":
                        UpdateVoiceCommand(this, "color frame");
                        UpdateAgleViewByVoice(this, AgleView.KinectColor);
                        break;

                    case "DEPTHFRAME":
                        UpdateVoiceCommand(this, "depth frame");
                        UpdateAgleViewByVoice(this, AgleView.KinectDepth);
                        break;

                    case "INFRAREDFRAME":
                        UpdateVoiceCommand(this, "infrared frame");
                        UpdateAgleViewByVoice(this, AgleView.KinectInfrared);
                        break;
                    case "MAINFORWARD":
                        UpdateVoiceCommand(this, "Main Forward");
                        UpdateAgleViewByVoice(this, AgleView.MainFront);
                        break;
                }
            }
        }

        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
        }

        internal void TerminateVoiceControl()
        {
            if (null != this.convertStream)
            {
                this.convertStream.SpeechActive = false;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= this.SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }
    }
}
