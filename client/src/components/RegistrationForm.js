import React, { useState } from 'react';
import { Form, Button } from 'react-bootstrap';
import axios from 'axios';
import { Link } from 'react-router-dom';

const RegistrationForm = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleRegisterSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post('https://localhost:7010/api/User/Register', { username, password });
      console.log(response.data);
    } catch (error) {
      console.log(error.response.data);
    }
  };

  return (
    <div className="d-flex justify-content-center">
      <div className="text-center">
        <h2>Register</h2>
      </div>
      <Form onSubmit={handleRegisterSubmit}>
        <Form.Group controlId="formBasicUsername">
          <Form.Label>Username</Form.Label>
          <Form.Control type="text" placeholder="Enter username" value={username} onChange={(e) => setUsername(e.target.value)} />
        </Form.Group>

        <Form.Group controlId="formBasicPassword">
          <Form.Label>Password</Form.Label>
          <Form.Control type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </Form.Group>

        <Button variant="primary" type="submit">
        <Link to="/login">Register</Link>
        </Button>
        <Link to="/login">Already have an account? Log in</Link>
      </Form>
    </div>
  );
};

export default RegistrationForm;
