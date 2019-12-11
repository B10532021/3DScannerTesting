#ifndef LIBOC_LIBRARY_H
#define LIBOC_LIBRARY_H

#include <string>
#include <vector>

namespace oc {

    class Library {
    public:
        Library();
        void OnARServiceConnected(std::string dataset, int mode);
        int OnBegin(void* session, void* frame);
        std::vector<int> OnDataGeomColor();
        std::vector<int> OnDataGeomFace();
        std::vector<float> OnDataGeomNormal();
        std::vector<float> OnDataGeomVertex();
        std::string OnDataIndex(int index);
        void OnEnd();
        void OnPixels(int width, int height, int8_t* pixels, int scale);
        void OnProcess();
        void OnSurfaceChanged(int width, int height);

        void Clear();
        bool IsSaveFinished();
        bool IsSaveSuccessful();
        void Save(std::string filename);
        void SetActive(bool on);
        void SetExtendingPointCloud(bool on);
        void SetFillingHoles(bool on);
        void SetMeshing(double res, double dmin, double dmax, int noise);
    };
}

#endif
