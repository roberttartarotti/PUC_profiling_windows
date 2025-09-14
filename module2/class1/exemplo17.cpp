/*
================================================================================
ATIVIDADE PRÁTICA 17 - DEEP RECURSION STACK OVERFLOW (C++)
================================================================================

OBJETIVO:
- Demonstrar problemas de performance com deep recursion
- Usar CPU profiler para identificar stack overflow risks
- Otimizar convertendo recursion para iteration
- Medir diferença entre recursive vs iterative approaches

PROBLEMA:
- Deep recursion consome stack space rapidamente
- Risk of stack overflow em datasets grandes
- CPU Profiler mostrará high function call overhead

SOLUÇÃO:
- Converter para iterative solution usando explicit stack
- Tail recursion optimization onde aplicável

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
using namespace std;

class TreeNode {
public:
    int value;
    vector<TreeNode*> children;
    
    TreeNode(int val) : value(val) {}
    
    ~TreeNode() {
        for (auto child : children) {
            delete child;
        }
    }
    
    void addChild(TreeNode* child) {
        children.push_back(child);
    }
};

// PERFORMANCE ISSUE: Deep recursive tree traversal
long long recursiveTreeSum(TreeNode* node) {
    if (!node) return 0;
    
    long long sum = node->value;
    
    // Recursive calls for each child - can cause stack overflow
    for (TreeNode* child : node->children) {
        sum += recursiveTreeSum(child); // Deep recursion risk
    }
    
    return sum;
}

TreeNode* createDeepTree(int depth, int branchingFactor) {
    if (depth <= 0) return nullptr;
    
    TreeNode* root = new TreeNode(depth);
    
    for (int i = 0; i < branchingFactor; i++) {
        TreeNode* child = createDeepTree(depth - 1, branchingFactor);
        if (child) {
            root->addChild(child);
        }
    }
    
    return root;
}

void demonstrateDeepRecursion() {
    cout << "Starting deep recursion demonstration..." << endl;
    cout << "Monitor CPU profiler - should see high function call overhead and stack usage" << endl;
    
    const int TREE_DEPTH = 15;        // Deep tree
    const int BRANCHING_FACTOR = 3;    // 3 children per node
    
    cout << "Creating tree with depth " << TREE_DEPTH << " and branching factor " << BRANCHING_FACTOR << endl;
    TreeNode* root = createDeepTree(TREE_DEPTH, BRANCHING_FACTOR);
    
    if (!root) {
        cout << "Failed to create tree!" << endl;
        return;
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    // PERFORMANCE ISSUE: Deep recursive traversal
    long long sum = recursiveTreeSum(root);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Deep recursion completed in: " << duration.count() << " ms" << endl;
    cout << "Tree sum: " << sum << endl;
    cout << "Warning: Risk of stack overflow with deeper trees!" << endl;
    
    delete root;
}

int main() {
    cout << "Starting deep recursion demonstration..." << endl;
    cout << "Task: Computing sum of deep tree using recursion" << endl;
    cout << "Monitor CPU Usage Tool for function call overhead and stack usage" << endl << endl;
    
    demonstrateDeepRecursion();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- High function call overhead" << endl;
    cout << "- Stack usage growth" << endl;
    cout << "- Risk of stack overflow with deeper recursion" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR ITERATIVE APPROACH)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <stack>
using namespace std;

class TreeNode {
public:
    int value;
    vector<TreeNode*> children;
    
    TreeNode(int val) : value(val) {}
    
    ~TreeNode() {
        for (auto child : children) {
            delete child;
        }
    }
    
    void addChild(TreeNode* child) {
        children.push_back(child);
    }
};

// CORREÇÃO: Iterative tree traversal using explicit stack
long long iterativeTreeSum(TreeNode* root) {
    if (!root) return 0;
    
    long long sum = 0;
    stack<TreeNode*> nodeStack; // Explicit stack replaces call stack
    nodeStack.push(root);
    
    while (!nodeStack.empty()) {
        TreeNode* current = nodeStack.top();
        nodeStack.pop();
        
        sum += current->value;
        
        // Add children to stack (no recursive calls)
        for (TreeNode* child : current->children) {
            if (child) {
                nodeStack.push(child);
            }
        }
    }
    
    return sum;
}

TreeNode* createDeepTreeIterative(int depth, int branchingFactor) {
    if (depth <= 0) return nullptr;
    
    // CORREÇÃO: Create tree iteratively to avoid stack overflow during creation
    TreeNode* root = new TreeNode(depth);
    
    struct NodeToCreate {
        TreeNode* parent;
        int remainingDepth;
    };
    
    stack<NodeToCreate> creationStack;
    creationStack.push({root, depth - 1});
    
    while (!creationStack.empty()) {
        NodeToCreate current = creationStack.top();
        creationStack.pop();
        
        if (current.remainingDepth <= 0) continue;
        
        for (int i = 0; i < branchingFactor; i++) {
            TreeNode* child = new TreeNode(current.remainingDepth);
            current.parent->addChild(child);
            
            if (current.remainingDepth > 1) {
                creationStack.push({child, current.remainingDepth - 1});
            }
        }
    }
    
    return root;
}

void demonstrateIterativeTraversal() {
    cout << "Starting iterative traversal demonstration..." << endl;
    cout << "Monitor CPU profiler - should see reduced function call overhead" << endl;
    
    const int TREE_DEPTH = 20;        // Even deeper tree - no stack overflow risk
    const int BRANCHING_FACTOR = 3;
    
    cout << "Creating tree iteratively with depth " << TREE_DEPTH << " and branching factor " << BRANCHING_FACTOR << endl;
    TreeNode* root = createDeepTreeIterative(TREE_DEPTH, BRANCHING_FACTOR);
    
    if (!root) {
        cout << "Failed to create tree!" << endl;
        return;
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Iterative traversal - no stack overflow risk
    long long sum = iterativeTreeSum(root);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Iterative traversal completed in: " << duration.count() << " ms" << endl;
    cout << "Tree sum: " << sum << endl;
    cout << "No stack overflow risk - can handle much deeper trees!" << endl;
    
    delete root;
}

int main() {
    cout << "Starting optimized iterative demonstration..." << endl;
    cout << "Task: Computing sum of deep tree using iteration" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstrateIterativeTraversal();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- No function call overhead" << endl;
    cout << "- Constant stack usage regardless of tree depth" << endl;
    cout << "- No risk of stack overflow" << endl;
    cout << "- Can handle much deeper data structures" << endl;
    cout << "- Better performance for deep traversals" << endl;
    
    return 0;
}

================================================================================
*/
