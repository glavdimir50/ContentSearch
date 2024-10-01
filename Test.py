import sys, os, nltk, json

from nltk.tokenize import sent_tokenize
from nltk.tokenize import word_tokenize
from nltk.corpus import stopwords
from nltk import FreqDist
from nltk.corpus import stopwords
from bs4 import BeautifulSoup

inputs = sys.argv[1].split('|')

def textCount(filePath):
    with open(filePath, mode='r', encoding="utf-8") as file:
        contents = file.read()
        content = ''
        if '.xml' in filePath:
            soup = BeautifulSoup(contents, 'lxml-xml')
            for abstract in soup.find_all('AbstractText'):
                content += abstract.get_text() + "\r\n"
        else:
            content = contents     

        words = word_tokenize(content)
        words = [word for word in words if any((ord(char) > 47 and ord(char) <58) or (ord(char) > 65 and ord(char) < 91) or (ord(char) > 96 and ord(char) < 122) for char in word)]
        stopWords = set(stopwords.words('english'))
        charCountIncludingSpaces = len(content.replace("\r\n", ""))
        charCountExcludingSpaces = len(content.replace(" ", ""))
        wordsCount = len(words)
        sentences = sent_tokenize(content)
        sentenceCount = len(sentences)
        nonAsciiChars = [char for char in content if ord(char) > 127]
        nonAsciiWords = [word for word in words if any(ord(char) > 127 for char in word)]
        words = word_tokenize(content.lower())
        normalized_words = [freqW for freqW in words if freqW.isalpha()]
        frequency_distribution = FreqDist(normalized_words)
        frequencyWords = dict(frequency_distribution.most_common(wordsCount))
        result_dict = {
            "FileName": filePath,
            "CharCountIncludingSpaces": charCountIncludingSpaces,
            "CharCountExcludingSpaces": charCountExcludingSpaces,
            "WordsCount": wordsCount,
            "SentenceCount": sentenceCount,
            "FrequencyWords": frequencyWords,
            "NonAsciiChars": len(nonAsciiChars),
            "NonAsciiWords": len(nonAsciiWords),
            "Content": content,
            "Stopwords": list(stopWords)
        }
        return result_dict
    
def generate_result(dic):
    result_json = json.dumps(dic, ensure_ascii=False, indent=4)
    return result_json

def IRAllFiles(input):
    filePaths = input[1].split('&')
    resultList = []
    for filePath in filePaths:
        resultList.append(textCount(filePath))
    return  resultList

def GetInput(inputstr):
    result = IRAllFiles(inputstr)
    dir = r'D:\TestDir'
    if not os.path.exists(dir):
       os.makedirs(dir)
    filePath = os.path.join(dir, "result.json")
    with open(filePath, 'w', encoding='utf-8') as json_file:
        json.dump(result, json_file, ensure_ascii=False, indent=4)
    print(filePath, end='')

GetInput(inputs)