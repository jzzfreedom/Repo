// KinectVisionWrapper.h

#pragma once
#include <opencv2/opencv.hpp>

using namespace System;
using namespace cv;

namespace KinectVisionWrapper {

	public ref class newFrameEventArgs : EventArgs
	{
		System::Drawing::Bitmap^ m_newBitmap;
		int m_agleViewCode;
	public:
		newFrameEventArgs(System::Drawing::Bitmap^ arg, int agleView)
		{
			m_newBitmap = arg;
			m_agleViewCode = agleView;
		}
		property System::Drawing::Bitmap^ NewBitmap
		{
			System::Drawing::Bitmap^ get() { return m_newBitmap; }
		}
		property int AgleViewCode
		{
			int get() { return m_agleViewCode; }
		}
	};

	public ref class KinectOpenCV
	{
		// TODO:  在此处添加此类的方法。

	public:

		KinectOpenCV();
		void Startup();
		void MainCameraTerminate();
		event System::EventHandler<newFrameEventArgs^>^ newFrameArrive;
		System::Drawing::Bitmap^ ConvertMatToBitmap(cv::Mat& cvImg);
		
		bool CheckMainFrontCameraAvailability();

	private:
		bool m_isMainThreadTerminated;
		Object^ terminateMutex;
	};


}
