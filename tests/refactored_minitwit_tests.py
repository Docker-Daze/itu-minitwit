# -*- coding: utf-8 -*-
"""
    MiniTwit Tests
    ~~~~~~~~~~~~~~

    Tests a MiniTwit application.

    This version mimics browser behavior by performing a GET first to initialize
    the Razor Page model, then a POST. (This is necessary in ASP.NET Core Razor Pages.)
    
    :refactored: (c) 2024 by HelgeCPH from Armin Ronacher's original unittest version
    :copyright: (c) 2010 by Armin Ronacher.
    :license: BSD, see LICENSE for more details.
"""
import re
import requests

BASE_URL = "http://localhost:5114"

def get_csrf_token(session, url):
    """Perform a GET request to the URL and extract the __RequestVerificationToken."""
    response = session.get(url)
    
    match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', response.text)
    if match:
        return match.group(1)
    else:
        print("CSRF token not found!")
        return None

def register(username, password, password2=None, email=None):
    """Register a user by first getting the registration page to obtain the token."""
    session = requests.Session()
    # Load the registration form to establish cookies and get the token.
    get_response = session.get(f'{BASE_URL}/register')
    token = get_csrf_token(session, f'{BASE_URL}/register')
    
    if password2 is None:
        password2 = password
    if email is None:
        email = username + '@example.com'
    
    data = {
        'Input.UserName': username,
        'Input.Email': email,
        'Input.Password': password,
        'Input.ConfirmPassword': password2,
        '__RequestVerificationToken': token
    }
    # Now POST the registration form with the token included.
    return session.post(f'{BASE_URL}/register', data=data, allow_redirects=True), session

def login(username, password):
    """Log in a user by loading the login page first, then posting the credentials with the token."""
    session = requests.Session()
    session.get(f'{BASE_URL}/login')
    token = get_csrf_token(session, f'{BASE_URL}/login')
    data = {
        'Input.UserName': username,
        'Input.Password': password,
        '__RequestVerificationToken': token
    }
    r = session.post(f'{BASE_URL}/login', data=data, allow_redirects=True)
    return r, session

def register_and_login(username, password):
    """Registers and then logs in using the same session."""
    reg_response, session = register(username, password)
    
    
    session.get(f'{BASE_URL}/login')
    token = get_csrf_token(session, f'{BASE_URL}/login')
    data = {
        'Input.UserName': username,
        'Input.Password': password,
        '__RequestVerificationToken': token
    }
    login_response = session.post(f'{BASE_URL}/login', data=data, allow_redirects=True)
    return login_response, session

def add_message(http_session, text):
    """Posts a message after loading the form to retrieve the token."""
    http_session.get(f'{BASE_URL}/')
    
    
    token = get_csrf_token(http_session, f'{BASE_URL}/')
    data = {'text': text, '__RequestVerificationToken': token}
    r = http_session.post(f'{BASE_URL}/add_message', data=data, allow_redirects=True)
    return r


# Testing functions

def test_register():
    """Make sure registering works."""
    r, _ = register('user12', 'Default12@')
   
    assert 'You were successfully registered and can login now' in r.text, f"Response was: {r.text}"
    
    # Attempt to register the same username again.
    r, _ = register('user12', 'Default12@')
    assert 'The username is already taken' in r.text, f"Response was: {r.text}"
    
    r, _ = register('', 'Default12@')
    assert 'You have to enter a username' in r.text, f"Response was: {r.text}"
    
    r, _ = register('meh', '')
    assert 'You have to enter a password' in r.text, f"Response was: {r.text}"
    
    r, _ = register('meh', 'Default12@', 'Default12@1')
    assert 'The two passwords do not match' in r.text, f"Response was: {r.text}"
    
    r, _ = register('meh', 'foo', email='broken')
    assert 'You have to enter a valid email address' in r.text, f"Response was: {r.text}"


def test_login_logout():
    """Make sure logging in and logging out works."""
    r, http_session = register_and_login('user12', 'Default12@')
    assert 'You were logged in' in r.text, f"Response was: {r.text}"
    
    r = logout(http_session)
    assert 'You were logged out' in r.text, f"Response was: {r.text}"
    
    r, _ = login('user12', 'wrongpassword')
    assert 'Invalid password' in r.text, f"Response was: {r.text}"
    
    r, _ = login('user2', 'wrongpassword')
    assert 'Invalid username' in r.text, f"Response was: {r.text}"


def test_message_recording():
    """Check if adding messages works."""
    _, http_session = register_and_login('foo', 'Default12@')
    add_message(http_session, 'test message 1')
    add_message(http_session, '<test message 2>')
    r = requests.get(f'{BASE_URL}/')
    assert 'test message 1' in r.text, f"Response was: {r.text}"
    assert '&lt;test message 2&gt;' in r.text, f"Response was: {r.text}"


def test_timelines():
    """Make sure that timelines work."""
    _, http_session = register_and_login('foo', 'Default12@')
    add_message(http_session, 'the message by foo')
    logout(http_session)
    
    _, http_session = register_and_login('bar', 'Default12@')
    add_message(http_session, 'the message by bar')
    r = http_session.get(f'{BASE_URL}/public')
    assert 'the message by foo' in r.text, f"Response was: {r.text}"
    assert 'the message by bar' in r.text, f"Response was: {r.text}"
    
    # Bar's timeline should initially show only his own message.
    r = http_session.get(f'{BASE_URL}/')
    assert 'the message by foo' not in r.text, f"Response was: {r.text}"
    assert 'the message by bar' in r.text, f"Response was: {r.text}"
    
    # Follow foo
    r = http_session.get(f'{BASE_URL}/foo/follow', allow_redirects=True)
    assert 'You are now following &#34;foo&#34;' in r.text, f"Response was: {r.text}"
    
    # After following, bar's timeline should include foo's message.
    r = http_session.get(f'{BASE_URL}/')
    assert 'the message by foo' in r.text, f"Response was: {r.text}"
    assert 'the message by bar' in r.text, f"Response was: {r.text}"
    
    # Check individual user pages.
    r = http_session.get(f'{BASE_URL}/bar')
    assert 'the message by foo' not in r.text, f"Response was: {r.text}"
    assert 'the message by bar' in r.text, f"Response was: {r.text}"
    r = http_session.get(f'{BASE_URL}/foo')
    assert 'the message by foo' in r.text, f"Response was: {r.text}"
    assert 'the message by bar' not in r.text, f"Response was: {r.text}"
    
    # Unfollow foo.
    r = http_session.get(f'{BASE_URL}/foo/unfollow', allow_redirects=True)
    assert 'You are no longer following &#34;foo&#34;' in r.text, f"Response was: {r.text}"
    r = http_session.get(f'{BASE_URL}/')
    assert 'the message by foo' not in r.text, f"Response was: {r.text}"
    assert 'the message by bar' in r.text, f"Response was: {r.text}"
