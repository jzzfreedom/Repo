// 这是主 DLL 文件。

#include "stdafx.h"

#include "KinectVisionWrapper.h"
#include <opencv2/opencv.hpp>

using namespace System;
using namespace System::Threading;
using namespace cv;
using namespace System::Drawing;

#define MainFrontCamera 0
#define BackwardCamera 1
#define AgleView_MainFront 3
#define AgleView_Forward 4
#define AgleView_Rear 5
#define AgleView_Left 6
#define AgleView_Right 7

namespace KinectVisionWrapper {
	KinectOpenCV::KinectOpenCV()
		: m_isMainThreadTerminated(false)
	{
		//terminateMutex = new Queue<int>;
	}

	void KinectOpenCV::Startup()
	{
		VideoCapture cap(MainFrontCamera); // open the default camera

		if (!cap.isOpened()) // check if we succeeded	
		namedWindow("edges", 1);

		bool terminate = false;

		while (!terminate)
		{
			Mat frame;
			Mat edges;

			cap >> frame; // get a new frame from camera

			cvtColor(frame, edges, CV_BGR2GRAY);

			GaussianBlur(edges, edges, cv::Size(7, 7), 1.5, 1.5);

			Canny(edges, edges, 0, 30, 3);

			//imshow("edges", edges);
			Bitmap^ newBitmap = ConvertMatToBitmap(edges);
			newFrameArrive(this, gcnew newFrameEventArgs(newBitmap, AgleView_MainFront));

			if (waitKey(30) >= 0) break;

			Monitor::Enter(this);
			if (m_isMainThreadTerminated)
			{
				terminate = true;
			}
			Monitor::Exit(this);
		}

	}
	void KinectOpenCV::MainCameraTerminate()
	{
		Monitor::Enter(this);
		m_isMainThreadTerminated = true;
		Monitor::Exit(this);
	}

	bool KinectOpenCV::CheckMainFrontCameraAvailability()
	{
		VideoCapture cap(MainFrontCamera);
		bool availability = cap.isOpened();
		cap.release();
		return availability;
	}

	Bitmap^ KinectOpenCV::ConvertMatToBitmap(cv::Mat& cvImg)
	{
		Bitmap^ bmpImg;

		//检查图像位深  
		if (cvImg.depth() != CV_8U)
		{
			//cout << "输入图像位深：" << cvImg.depth() << ". 只处理每通道8位深度的图像！" << endl;
			bmpImg = gcnew Bitmap(1, 1, Imaging::PixelFormat::Format8bppIndexed);
			return (bmpImg);
		}

		//彩色图像  
		if (cvImg.channels() == 3)
		{
			bmpImg = gcnew Bitmap(
				cvImg.cols,
				cvImg.rows,
				cvImg.step,
				Imaging::PixelFormat::Format24bppRgb,
				(System::IntPtr)cvImg.data);
		}
		//灰度图像  
		else if (cvImg.channels() == 1)
		{
			bmpImg = gcnew Bitmap(
				cvImg.cols,
				cvImg.rows,
				cvImg.step,
				Imaging::PixelFormat::Format8bppIndexed,
				(System::IntPtr)cvImg.data);
		}

		return (bmpImg);
	}
}
