# -*- coding: utf-8 -*-
"""
Created on Tue Nov 10 12:27:55 2020

@author: cgna
"""

import socket
import select
import time
import sys
import struct

class SocketCommunicator:
    
    def __init__(self, IP, PORT, BUFSIZE):
        self.data_dic = {}
        self.floatByteSize = 4
        self.IP = IP
        self.PORT = PORT
        self.BUFSIZE = BUFSIZE
        self.ADDR = (IP, PORT)
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.bind(self.ADDR)
        self.server_socket.listen(10)
        self.connection_list = [self.server_socket]
        print('==============================================')
        print('                Server start!')
        print('==============================================')
        print('Waiting for %s port connection.' % str(self.PORT))
   
    def receive_data(self):
        self.read_socket, self.write_socket, self.error_socket = select.select(self.connection_list, [], [], 10)
        
        data=[]
        for sock in self.read_socket:
            if sock == self.server_socket:
                clientSocket, addr_info = self.server_socket.accept()
                self.connection_list.append(clientSocket)
                print('[INFO][%s] client (%s) has been connected.' % (time.ctime(), addr_info[0]))
                
            else:
                message = sock.recv(self.BUFSIZE)
                message_len = struct.unpack("I", bytearray(message[:self.floatByteSize]))[0]
                message=message[self.floatByteSize:]
                while len(message) != message_len:
                    message += sock.recv(self.BUFSIZE)

                if message:
                    for i in range(0,(int)(message_len/self.floatByteSize)):
                        data.append(struct.unpack("f", bytearray(message[i*self.floatByteSize:(i+1)*self.floatByteSize]))[0])
                    self.data_dic[socket] = data

                else:
                    self.connection_list.remove(sock)
                    sock.close()
                    print('[INFO][%s] client has been disconnected.' % time.ctime())
        
    def send_data(self,sock,data_list):
        send_string = ''
        for data in data_list:
            send_string += str(data) + ','
        send_string=send_string[:-1]
        sock.send(send_string.encode())


if __name__=="__main__":
    socket_communicator = SocketCommunicator('127.0.0.1',56789,1024)
    while socket_communicator.connection_list:
        try:
            socket_communicator.receive_data()
            for sock in socket_communicator.read_socket:
                if sock != socket_communicator.server_socket:
                    data_list=[]
                    data_list.append(1)
                    socket_communicator.send_data(sock,data_list)
                        
        except KeyboardInterrupt:
            # 부드럽게 종료하기
            socket_communicator.server_socket.close()
            sys.exit()

    
    
    
    
    