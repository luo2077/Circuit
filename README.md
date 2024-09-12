# Circuit简介
这个VS项目用于构造基础的直流电路(电阻，理想电压源，理想电压表，理想电流表，理想导线)，并提供方法求解支路的电流。
This is a class for solving resistive DC circuits

# 求解电路原理：
用了支路电流法，用电流为未知量，列出kcl和kvl电路矩阵方程组，用开源Math.Net库中的提供的矩阵方法求解矩阵方程得出电流矩阵。

# 适用范围：
仅用于初中阶段的直流电路求解，一般只有电源，电阻，电压表，电流表，导线。

#示例
下面是一个需要求接的电路
![image](https://github.com/luo2077/Circuit/tree/main/img/eg_circuit.png)
在代码中构造一个电路类，插入所有支路，包括支路的已知属性(电阻，电动势)
![image](https://github.com/luo2077/Circuit/tree/main/img/main.png)
运行程序，在控制台就能看见求解结果
![image](https://github.com/luo2077/Circuit/tree/main/img/output.png)
