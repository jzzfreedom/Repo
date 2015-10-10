#include <iostream>
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
//#include <core.hpp>
//#include <highgui.hpp>

using namespace std;
using namespace cv;

int main()
{
	IplImage* pFrame = NULL;

	//获取摄像头
	CvCapture* pCapture = cvCreateCameraCapture(0);

	//创建窗口
	cvNamedWindow("Video", 1);

	//显示视屏
	while (1)
	{
		pFrame = cvQueryFrame(pCapture);
		if (!pFrame)break;
		cvShowImage("Video", pFrame);
		char c = cvWaitKey(33);
		if (c == 27)break;
	}
	cvReleaseCapture(&pCapture);
	cvDestroyWindow("Video");
	return 0;
	//Mat img;
	//return 0;
}