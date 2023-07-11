#include <iostream>

class a1 {
private:
	int* data = NULL;
public:
	~a1() { Stop(); };

	virtual void Start() {
		if (data)
			delete[] data;

		data = new int[10000];
	}

	virtual void Stop() {
		if (data) {
			delete[] data;
			data = NULL;
		}
	}
};

class a2 : public a1 {
private:
	int* data2 = NULL;

public:

	void Start() {
		a1::Start();

		if (data2)
			delete[] data2;

		data2 = new int[10000];
		Stop(); // ну технически оно работает🌚
	}

	void Stop() override {
		a1::Stop();

		if (data2) {
			delete[] data2;
			data2 = NULL;
		}
	}
};

void Proc() {

	a1* A = new a2();
	A->Start();
	delete A;

}

int main()
{
	setlocale(LC_ALL, "Russian");
	for (;;) Proc();

}