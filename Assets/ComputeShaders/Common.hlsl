int _Size = 32;

static const uint numberOfThreads = 8;

int indexFromCoordinates(int x, int y, int z)
{
    return x + (_Size * (y + (_Size * z)));
}


