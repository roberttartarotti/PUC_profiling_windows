/*
================================================================================
ATIVIDADE PRÁTICA 20 - IMAGE PROCESSING PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar algoritmos ineficientes de processamento de imagem
- Usar CPU profiler para identificar gargalos em pixel manipulation
- Otimizar usando vectorized operations e SIMD quando possível
- Medir diferença entre pixel-by-pixel vs batch processing

PROBLEMA:
- Processing pixels um-por-um é muito lento
- Lack of spatial/temporal locality em memory access
- CPU Profiler mostrará tempo gasto em nested loops

SOLUÇÃO:
- Batch processing de multiple pixels
- Optimized memory access patterns
- Use vectorized operations para SIMD

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
#include <algorithm>
using namespace std;

struct Pixel {
    unsigned char r, g, b;
    
    Pixel() : r(0), g(0), b(0) {}
    Pixel(unsigned char red, unsigned char green, unsigned char blue) : r(red), g(green), b(blue) {}
};

class Image {
private:
    vector<vector<Pixel>> pixels;
    int width, height;
    
public:
    Image(int w, int h) : width(w), height(h) {
        pixels.resize(height, vector<Pixel>(width));
    }
    
    void fillRandom() {
        random_device rd;
        mt19937 gen(rd());
        uniform_int_distribution<> dis(0, 255);
        
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                pixels[y][x] = Pixel(dis(gen), dis(gen), dis(gen));
            }
        }
    }
    
    Pixel& getPixel(int x, int y) { return pixels[y][x]; }
    const Pixel& getPixel(int x, int y) const { return pixels[y][x]; }
    int getWidth() const { return width; }
    int getHeight() const { return height; }
};

void demonstrateInefficicentImageProcessing() {
    cout << "Starting inefficient image processing demonstration..." << endl;
    cout << "Monitor CPU profiler - should see time spent in nested pixel loops" << endl;
    
    const int IMAGE_WIDTH = 2000;
    const int IMAGE_HEIGHT = 1500;
    
    Image sourceImage(IMAGE_WIDTH, IMAGE_HEIGHT);
    Image processedImage(IMAGE_WIDTH, IMAGE_HEIGHT);
    
    cout << "Generating random image data..." << endl;
    sourceImage.fillRandom();
    
    auto start = chrono::high_resolution_clock::now();
    
    // PERFORMANCE ISSUE: Processing pixels one by one with multiple passes
    
    // Pass 1: Convert to grayscale
    for (int y = 0; y < IMAGE_HEIGHT; y++) {
        for (int x = 0; x < IMAGE_WIDTH; x++) {
            const Pixel& src = sourceImage.getPixel(x, y);
            // Inefficient: accessing same pixel multiple times in different passes
            unsigned char gray = static_cast<unsigned char>(0.299 * src.r + 0.587 * src.g + 0.114 * src.b);
            processedImage.getPixel(x, y) = Pixel(gray, gray, gray);
        }
        
        if (y % 200 == 0) {
            cout << "Grayscale conversion: " << y << "/" << IMAGE_HEIGHT << " rows" << endl;
        }
    }
    
    // Pass 2: Apply brightness adjustment (another full image scan)
    for (int y = 0; y < IMAGE_HEIGHT; y++) {
        for (int x = 0; x < IMAGE_WIDTH; x++) {
            Pixel& pixel = processedImage.getPixel(x, y);
            // PERFORMANCE ISSUE: Multiple memory accesses, cache misses
            pixel.r = min(255, (int)pixel.r + 30);
            pixel.g = min(255, (int)pixel.g + 30);
            pixel.b = min(255, (int)pixel.b + 30);
        }
    }
    
    // Pass 3: Simple blur filter (very inefficient implementation)
    Image blurredImage(IMAGE_WIDTH, IMAGE_HEIGHT);
    for (int y = 1; y < IMAGE_HEIGHT - 1; y++) {
        for (int x = 1; x < IMAGE_WIDTH - 1; x++) {
            // PERFORMANCE ISSUE: Many individual pixel accesses per output pixel
            int totalR = 0, totalG = 0, totalB = 0;
            for (int dy = -1; dy <= 1; dy++) {
                for (int dx = -1; dx <= 1; dx++) {
                    const Pixel& p = processedImage.getPixel(x + dx, y + dy);
                    totalR += p.r;
                    totalG += p.g;
                    totalB += p.b;
                }
            }
            blurredImage.getPixel(x, y) = Pixel(totalR / 9, totalG / 9, totalB / 9);
        }
        
        if (y % 200 == 0) {
            cout << "Blur filter: " << y << "/" << IMAGE_HEIGHT << " rows" << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Inefficient image processing completed in: " << duration.count() << " ms" << endl;
    cout << "Image size: " << IMAGE_WIDTH << "x" << IMAGE_HEIGHT << " pixels" << endl;
    cout << "Total pixel operations: " << (IMAGE_WIDTH * IMAGE_HEIGHT * 3) << " (3 passes)" << endl;
    cout << "Multiple passes caused cache misses and redundant memory access" << endl;
}

int main() {
    cout << "Starting image processing performance demonstration..." << endl;
    cout << "Task: Processing large image with pixel-by-pixel operations" << endl;
    cout << "Monitor CPU Usage Tool for nested loop performance" << endl << endl;
    
    demonstrateInefficicentImageProcessing();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Time spent in nested pixel loops" << endl;
    cout << "- Cache miss patterns from multiple passes" << endl;
    cout << "- Memory access overhead" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR VECTORIZED PROCESSING)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
#include <algorithm>
#include <cstring>
using namespace std;

struct Pixel {
    unsigned char r, g, b;
    
    Pixel() : r(0), g(0), b(0) {}
    Pixel(unsigned char red, unsigned char green, unsigned char blue) : r(red), g(green), b(blue) {}
};

class OptimizedImage {
private:
    vector<Pixel> pixels; // CORREÇÃO: Single contiguous array for better cache locality
    int width, height;
    
public:
    OptimizedImage(int w, int h) : width(w), height(h) {
        pixels.resize(width * height);
    }
    
    void fillRandom() {
        random_device rd;
        mt19937 gen(rd());
        uniform_int_distribution<> dis(0, 255);
        
        for (auto& pixel : pixels) {
            pixel = Pixel(dis(gen), dis(gen), dis(gen));
        }
    }
    
    Pixel& getPixel(int x, int y) { return pixels[y * width + x]; }
    const Pixel& getPixel(int x, int y) const { return pixels[y * width + x]; }
    Pixel* getData() { return pixels.data(); }
    int getWidth() const { return width; }
    int getHeight() const { return height; }
    size_t getPixelCount() const { return pixels.size(); }
};

void demonstrateOptimizedImageProcessing() {
    cout << "Starting optimized image processing demonstration..." << endl;
    cout << "Monitor CPU profiler - should see improved cache utilization" << endl;
    
    const int IMAGE_WIDTH = 2000;
    const int IMAGE_HEIGHT = 1500;
    
    OptimizedImage sourceImage(IMAGE_WIDTH, IMAGE_HEIGHT);
    OptimizedImage resultImage(IMAGE_WIDTH, IMAGE_HEIGHT);
    
    cout << "Generating random image data..." << endl;
    sourceImage.fillRandom();
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Single pass processing with vectorized operations
    Pixel* src = sourceImage.getData();
    Pixel* dst = resultImage.getData();
    size_t pixelCount = sourceImage.getPixelCount();
    
    // CORREÇÃO: Process multiple pixels in single pass
    for (size_t i = 0; i < pixelCount; i++) {
        // Combined operation: grayscale conversion + brightness adjustment
        unsigned char gray = static_cast<unsigned char>(0.299 * src[i].r + 0.587 * src[i].g + 0.114 * src[i].b);
        
        // Apply brightness in same operation
        gray = min(255, (int)gray + 30);
        
        dst[i] = Pixel(gray, gray, gray);
        
        if (i % (pixelCount / 10) == 0) {
            cout << "Vectorized processing: " << (i * 100 / pixelCount) << "% complete" << endl;
        }
    }
    
    // CORREÇÃO: Optimized blur with better memory access pattern
    OptimizedImage blurredImage(IMAGE_WIDTH, IMAGE_HEIGHT);
    Pixel* blurDst = blurredImage.getData();
    
    // Process in blocks for better cache utilization
    const int BLOCK_SIZE = 64;
    
    for (int blockY = 1; blockY < IMAGE_HEIGHT - 1; blockY += BLOCK_SIZE) {
        for (int blockX = 1; blockX < IMAGE_WIDTH - 1; blockX += BLOCK_SIZE) {
            
            int maxY = min(blockY + BLOCK_SIZE, IMAGE_HEIGHT - 1);
            int maxX = min(blockX + BLOCK_SIZE, IMAGE_WIDTH - 1);
            
            for (int y = blockY; y < maxY; y++) {
                for (int x = blockX; x < maxX; x++) {
                    int totalR = 0, totalG = 0, totalB = 0;
                    
                    // CORREÇÃO: Cache-friendly access pattern within block
                    for (int dy = -1; dy <= 1; dy++) {
                        for (int dx = -1; dx <= 1; dx++) {
                            const Pixel& p = dst[(y + dy) * IMAGE_WIDTH + (x + dx)];
                            totalR += p.r;
                            totalG += p.g;
                            totalB += p.b;
                        }
                    }
                    
                    blurDst[y * IMAGE_WIDTH + x] = Pixel(totalR / 9, totalG / 9, totalB / 9);
                }
            }
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Optimized image processing completed in: " << duration.count() << " ms" << endl;
    cout << "Image size: " << IMAGE_WIDTH << "x" << IMAGE_HEIGHT << " pixels" << endl;
    cout << "Optimizations: Single pass + vectorized + blocked processing" << endl;
}

void demonstrateSIMDStyleProcessing() {
    cout << "Starting SIMD-style batch processing..." << endl;
    
    const int IMAGE_WIDTH = 2000;
    const int IMAGE_HEIGHT = 1500;
    
    OptimizedImage image(IMAGE_WIDTH, IMAGE_HEIGHT);
    image.fillRandom();
    
    auto start = chrono::high_resolution_clock::now();
    
    Pixel* pixels = image.getData();
    size_t pixelCount = image.getPixelCount();
    
    // CORREÇÃO: Process pixels in batches of 4 (simulate SIMD)
    const size_t BATCH_SIZE = 4;
    
    for (size_t i = 0; i < pixelCount - BATCH_SIZE + 1; i += BATCH_SIZE) {
        // Process 4 pixels simultaneously (SIMD-style)
        for (size_t j = 0; j < BATCH_SIZE; j++) {
            Pixel& p = pixels[i + j];
            
            // Vectorizable operations
            unsigned char gray = static_cast<unsigned char>(0.299 * p.r + 0.587 * p.g + 0.114 * p.b);
            p.r = p.g = p.b = min(255, (int)gray + 20);
        }
    }
    
    // Handle remaining pixels
    for (size_t i = (pixelCount / BATCH_SIZE) * BATCH_SIZE; i < pixelCount; i++) {
        Pixel& p = pixels[i];
        unsigned char gray = static_cast<unsigned char>(0.299 * p.r + 0.587 * p.g + 0.114 * p.b);
        p.r = p.g = p.b = min(255, (int)gray + 20);
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "SIMD-style processing completed in: " << duration.count() << " ms" << endl;
    cout << "Batch size: " << BATCH_SIZE << " pixels per iteration" << endl;
}

int main() {
    cout << "Starting optimized image processing demonstration..." << endl;
    cout << "Task: Efficient image processing with vectorized operations" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstrateOptimizedImageProcessing();
    cout << endl;
    demonstrateSIMDStyleProcessing();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Contiguous memory layout improves cache locality" << endl;
    cout << "- Single-pass processing reduces memory access" << endl;
    cout << "- Blocked processing optimizes cache utilization" << endl;
    cout << "- Vectorized operations enable SIMD optimizations" << endl;
    cout << "- Dramatically faster image processing pipeline" << endl;
    
    return 0;
}

================================================================================
*/
