
const express = require('express');
const app = express();
const http = require('http');
const server = http.createServer(app);
const port = 7777;
const { Server } = require("socket.io");
const io = new Server(server);

io.use((socket, next) => {
  if (socket.handshake.query.token === "UNITY" && socket.handshake.query.version === "0.1") {
    next();
  } else {
    next(new Error("인증 오류 "));
  }
});
var Users = [];
var Rooms = [];

io.on('connection', socket => {

  Users[socket.id] = {
    id: socket.id,
    nickname: "",
    Room: "",
  }

  function RoomResetGo() {

    var roomcheck = [];

    for (room in Rooms) {
      roomcheck.push({
        currentCnt: Rooms[room].currentCnt,
        RoomMaxCnt: Rooms[room].maxCnt,
        name: room
      })

    }
    io.emit('RoomReset', roomcheck)
  }


  socket.on('LoginCheck', name => {
    //접속하기 버튼을 누르면 입장합니다.

    var check = true;
    //변수 하나를 생성

    for (var k in Users) {
      if (Users[k].nickname == name) {
        check = false;
        break;
      }
    }
    //닉네임이 있는지 없는지 파악합니다.

    if (check) {
      //닉네임이 없다면 생성
      Users[socket.id].nickname = name
      //nickname 설정
      console.log(name + ": 로비진입 성공!")
      socket.emit('Login')
    }
    else {
      //닉네임이 있다면 오류
      console.log("닉네임 겹침!")
      socket.emit('LoginFailed')
    }
  })



  socket.on('JoinRoomCheck', (roomname) => {

    if (roomname in Rooms && Rooms[roomname].currentCnt < Rooms[roomname].maxCnt) {

      socket.join(roomname)
      socket.emit('Join', roomname)
      Users[socket.id].Room = roomname
      Rooms[roomname].currentCnt++

      var check = []
      socket.adapter.rooms.get(roomname).forEach((a) => {
        check.push(Users[a].nickname)
      })

      socket.to(roomname).emit('PlayerReset', check)
      RoomResetGo()
    }
    else {
      socket.emit('JoinFailed')
    }
  })
  socket.on('CreateCheck', (data, data2) => {
    if (data in Rooms) {
      //방이 있는지 없는지 확인
      console.log(" 방이름 겹침!")
      socket.emit('CreateFailed')
      //방생성 실패

    }
    else {
      //방생성 성공
      socket.join(data);
      //들어갑니다.

      Users[socket.id].Room = data


      Rooms[data] = {
        currentCnt: 1,
        maxCnt: Number(data2)
      }

      console.log(data + ": 방진입 성공!")

      socket.emit('Create')
      //성공했다고 이벤트를 보냅니다.


      RoomResetGo()
      //방 목록을 전부 보내는 이벤트를 실행합니다.
    }

  })
  socket.on('LeaveRoomCheck', (data, data2) => {
    //방을 나갑니다

    socket.leave(data)
    //leave를 사용합니다.

    if (Number(data2) <= 1) {
      //현재 방인원이 1이라면 삭제를 시킵니다.
      delete Rooms[Users[socket.id].Room]
    }
    else {

      Rooms[data].currentCnt--
      //방 인원 하나 뺍니다


      var check = []
      socket.adapter.rooms.get(data).forEach((a) => {
        check.push(Users[a].nickname)
      })
      socket.to(data).emit('PlayerReset', check)
      //현재 방인원 플레이어 목록을 갱신 시켜줍니다.

    }
    RoomResetGo()
    socket.emit('LeaveRoom')
    Users[socket.id].Room = ""
  })
  socket.on('RoomListCheck', (data) => {
    if (socket.adapter.rooms.size == 1) {
      return
    }
    var roomcheck = [];

    for (room in Rooms) {
      roomcheck.push({
        currentCnt: Rooms[room].currentCnt,
        RoomMaxCnt: Rooms[room].maxCnt,
        name: room
      })

    }

    console.log(roomcheck)
    socket.emit('RoomList', roomcheck)
  })


  socket.on('Chat', (nick, text, room) => {
    //채팅을 보냅니다.

    console.log(nick + ": " + text)
    socket.to(room).emit('ChatGet', nick, text)
    //보인을 제외한 방에 존재한 사람들에게 보냅니다.

  })
  socket.on('ChatCheck', (data) => {


    var check = []
    socket.adapter.rooms.get(data).forEach((a) => {
      check.push(Users[a].nickname)
    })
    //방안에있는 플레이어들의 목록을 불러옵니다.


    socket.emit('ChatOn', check)
  })
  console.log("연결함 : " + socket.id);


  socket.on('disconnect', zz => {
    console.log("연결끊김 : " + socket.id);
    //console.log(Users[socket.id].Room);
    if (Users[socket.id].Room != "") {
      //해당 유저가 방안이라면 
      if (Rooms[Users[socket.id].Room].currentCnt == 1) {
        //인원이 1이라면 삭제
        delete Rooms[Users[socket.id].Room]
      }
      else {
        Rooms[Users[socket.id].Room].currentCnt--
        //룸 인원 빼기


        var check = []
        socket.adapter.rooms.get(Users[socket.id].Room).forEach((a) => {
          check.push(Users[a].nickname)
        })
        socket.to(Users[socket.id].Room).emit('PlayerReset', check)
        //그 안에있는 사람들 플레이어 목록 갱신



        RoomResetGo()
        //방 리셋 



      }
    }

    delete Users[socket.id]
    //유저 정보 삭제
  })



});




server.listen(port, () => {
  console.log('listening on *:' + port);
});



