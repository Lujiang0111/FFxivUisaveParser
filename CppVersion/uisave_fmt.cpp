#include "uisave_fmt.h"
#include <iostream>
#include <fstream>

UisaveFmt::UisaveFmt(const std::string &file_name) :
    file_name_(file_name)
{

}

UisaveFmt::~UisaveFmt()
{

}

bool UisaveFmt::Init()
{
    std::ifstream fin(file_name_.c_str(), std::ios::binary);
    if (!fin.is_open())
    {
        std::cerr << "cannot open file: " << file_name_;
        return false;
    }

    fin.seekg(0, fin.end);
    size_t data_size = static_cast<size_t>(fin.tellg());

    fin.seekg(0, fin.beg);
    raw_data_.resize(data_size);
    fin.read(reinterpret_cast<char *>(&raw_data_[0]), data_size);
    return true;
}

bool UisaveFmt::Parse()
{
    std::cout << "raw data length=" << raw_data_.size() << std::endl;
    raw_data_reader_ = std::make_shared<ByteReader>(&raw_data_[0], raw_data_.size());

    // Parse header
    if (!raw_data_reader_->CanRead(16))
    {
        std::cerr << "error: insufficient data to read raw data header";
        return false;
    }
    raw_data_reader_->Read(8);
    uint32_t decrypted_length = static_cast<uint32_t>(raw_data_reader_->Read(4));
    raw_data_reader_->Read(4);

    // decrypt data
    if (!raw_data_reader_->CanRead(decrypted_length))
    {
        std::cerr << "error: nnsufficient data to read encrypted data";
        return false;
    }
    Decrypt(decrypted_length);
    std::cout << "decrypted data length=" << decrypted_data_.size() << std::endl;
    decrypted_data_reader_ = std::make_shared<ByteReader>(&decrypted_data_[0], decrypted_data_.size());

    // Parse Content ID
    if (!decrypted_data_reader_->CanRead(16))
    {
        std::cerr << "error: insufficient data to read raw data header";
        return false;
    }
    decrypted_data_reader_->Read(8);
    uint64_t content_id = decrypted_data_reader_->Read(8);
    std::cout << "content id=0x" << std::hex << content_id << std::dec << std::endl;

    // Parse sections
    while (decrypted_data_reader_->CanRead(16))
    {
        // Parse section header
        uint16_t section_index = static_cast<uint16_t>(decrypted_data_reader_->Read(2));
        decrypted_data_reader_->Read(6);
        uint32_t section_data_length = static_cast<uint32_t>(decrypted_data_reader_->Read(4));
        decrypted_data_reader_->Read(4);
        std::cout << "section index=" << section_index << ", section data length=" << section_data_length << std::endl;

        // Parse section data
        if (!decrypted_data_reader_->CanRead(section_data_length))
        {
            std::cerr << "error: insufficient data to read section data";
            return false;
        }

        switch (section_index)
        {
        case 0x11:
            decrypted_data_reader_->ReadArray(section_data_length, wp_section_data_);
            ParseWaymarkPresets();
            break;
        default:
            decrypted_data_reader_->Read(section_data_length);
            break;
        }

        // Parse trailing data
        if (!decrypted_data_reader_->CanRead(4))
        {
            std::cerr << "error: insufficient data to read trailing data";
            return false;
        }
        decrypted_data_reader_->Read(4);
    }

    return true;
}

void UisaveFmt::Decrypt(size_t size)
{
    decrypted_data_.resize(size);
    for (size_t i = 0; i < size; ++i)
    {
        decrypted_data_[i] = (static_cast<uint8_t>(raw_data_reader_->Read(1)) ^ 0x31);
    }
}

constexpr const char *Waymarks[] =
{
    "A", "B", "C", "D", "1", "2", "3", "4",
};

void UisaveFmt::ParseWaymarkPresets()
{
    wp_section_data_reader_ = std::make_shared<ByteReader>(&wp_section_data_[0], wp_section_data_.size());
    wp_section_data_reader_->Read(16);

    double x, y, z;
    uint8_t enable_flag;
    uint16_t zone_id;
    uint32_t timestamp;
    std::cout << "-------------------------------------------" << std::endl;
    while (wp_section_data_reader_->CanRead(104))
    {
        for (int i = 0; i < 8; ++i)
        {
            x = static_cast<int>(wp_section_data_reader_->Read(4)) / 1000.0;
            y = static_cast<int>(wp_section_data_reader_->Read(4)) / 1000.0;
            z = static_cast<int>(wp_section_data_reader_->Read(4)) / 1000.0;
            std::cout << "Waymark " << Waymarks[i] << ": " << x << ", " << y << ", " << z << std::endl;
        }
        enable_flag = static_cast<uint8_t>(wp_section_data_reader_->Read(1));
        wp_section_data_reader_->Read(1);
        zone_id = static_cast<uint16_t>(wp_section_data_reader_->Read(2));
        timestamp = static_cast<uint32_t>(wp_section_data_reader_->Read(4));
        std::cout << "enable flag: 0x" << std::hex << static_cast<uint16_t>(enable_flag) << std::dec << std::endl;
        std::cout << "zone id: " << zone_id << std::endl;
        std::cout << "timestamp: " << timestamp << std::endl;
        std::cout << "-------------------------------------------" << std::endl;
    }
}
